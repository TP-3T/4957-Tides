using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using TTT.DataClasses.HexData;
using UnityEngine;

// references:
// https://onnxruntime.ai/docs/get-started/with-csharp.html
// https://github.com/asus4/onnxruntime-unity-examples/tree/main
//
namespace TTT.ClimateModel
{
    public sealed class MLModel : IDisposable
    {
        private static readonly string _MODEL_FILE_NAME = "xgboost_model.onnx";

        private static readonly string _MODEL_PATH;

        // min value in training set for global mean sea level (mm)
        public static readonly double TRAINING_DATASET_GMSL_LOWER_BOUND =
            -1.3122;

        // max value in training set for global mean sea level (mm)
        public static readonly double TRAINING_DATASET_GMSL_UPPER_BOUND =
            165.2076002;

        // historical climate data queue should always have length 11
        private static readonly int WORLD_STATE_ONE_YEAR_AGO_INDEX = 9;
        private static readonly int WORLD_STATE_FIVE_YEARS_AGO_INDEX = 5;
        private static readonly int WORLD_STATE_TEN_YEARS_AGO_INDEX = 0;

        private readonly InferenceSession _onnxInferenceSession;

        private readonly string _tensorInputName;

        // only one tensor output so don't need this for now
        // private readonly string _tensorOutputName;

        static MLModel()
        {
            _MODEL_PATH = Path.Combine(
                Application.streamingAssetsPath,
                "MLModels",
                _MODEL_FILE_NAME
            );

            //todo: remove later once tested
            if (!File.Exists(_MODEL_PATH))
            {
                Debug.LogError($"ONNX model not found at path: {_MODEL_PATH}");
            }
        }

        public MLModel()
        {
            // Load the model and it's metadata
            _onnxInferenceSession = new InferenceSession(_MODEL_PATH);
            _tensorInputName = _onnxInferenceSession.InputMetadata.Keys.First();
            // _tensorOutputName =  _onnxInferenceSession.OutputMetadata.Keys.First();
        }

        /// <summary>
        /// Uses the ML model to predicts the future sea level given the input vars
        /// References: https://onnxruntime.ai/docs/get-started/with-csharp.html
        /// </summary>
        /// <param name="input"></param>
        /// <param name="climateModelWorldStatesQueue"></param>
        /// <returns></returns>
        public float PredictFutureSeaLevel(
            ClimateModelInput input,
            Queue<WorldState> climateModelWorldStatesQueue
        )
        {
            float[] modelInputFeatures = CreateModelInputList(
                input,
                climateModelWorldStatesQueue
            );

            // creates a "tensor" - information about the input data used by the ml model
            var inputTensor = new DenseTensor<float>(
                modelInputFeatures,
                new[] { 1, modelInputFeatures.Length }
            );

            // creates a description of the tensor for the model to be able intepret
            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor(_tensorInputName, inputTensor),
            };

            // run the model inference (requires memory to be disposed after so added using)
            using var outputs = _onnxInferenceSession.Run(inputs);

            // outputs contains a sequence of maps, and we only need the first one
            // get the tensor contained in the NamedOnnxValue element
            Tensor<float> outputTensor = outputs.First().AsTensor<float>();

            // to access the inference result, need to convert the tensor to normal managed C# array, and then get the first (and only) element in it
            float futureSeaLevelPrediction = outputTensor.ToArray()[0];

            return futureSeaLevelPrediction;
        }

        /// <summary>
        /// Creates a list of model inputs derived from the inputs from the Unity game.
        /// </summary>
        private static float[] CreateModelInputList(
            ClimateModelInput input,
            Queue<WorldState> climateModelWorldStatesQueue
        )
        {
            // 10y ago
            WorldState worldState10YearsAgo =
                climateModelWorldStatesQueue.ElementAt(
                    WORLD_STATE_TEN_YEARS_AGO_INDEX
                );

            // 5y ago
            WorldState worldState5YearsAgo =
                climateModelWorldStatesQueue.ElementAt(
                    WORLD_STATE_FIVE_YEARS_AGO_INDEX
                );

            // 12m ago
            WorldState worldState12MonthsAgo =
                climateModelWorldStatesQueue.ElementAt(
                    WORLD_STATE_ONE_YEAR_AGO_INDEX
                );

            // this features list has to match the dataset column order
            // (the csv file is in ai repo, climate-prediction-training branch, training/data/interim folder)

            // For reference - mapping of feature indices
            /*
            "CO2 (ppm)": "f0",
            "TEMP (deg C)": "f1",
            "Absolute GMSL (mm) relative to Jan 1950": "f2",
            "CO2_12m_ago": "f3",
            "CO2_5y_ago": "f4",
            "CO2_10y_ago": "f5",
            "TEMP_12m_ago": "f6",
            "TEMP_5y_ago": "f7",
            "TEMP_10y_ago": "f8",
            "GMSL_12m_ago": "f9",
            "GMSL_5y_ago": "f10",
            "GMSL_10y_ago": "f11"
            */
            float[] features =
            {
                // Current year inputs
                (float)input.currAtmosphericCO2ConcentrationPpm,
                (float)input.currTemperatureCelsius,
                (float)input.currSeaLevelMM,
                // historical CO2 data
                worldState12MonthsAgo.Pollution,
                worldState5YearsAgo.Pollution,
                worldState10YearsAgo.Pollution,
                // historical temperature data
                worldState12MonthsAgo.Temp,
                worldState5YearsAgo.Temp,
                worldState10YearsAgo.Temp,
                // historical sea level data
                worldState12MonthsAgo.SeaLevel,
                worldState5YearsAgo.SeaLevel,
                worldState10YearsAgo.SeaLevel,
            };

            return features;
        }

        public void Dispose()
        {
            _onnxInferenceSession.Dispose();
        }
    }
}
