using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using TTT.DataClasses.ClimateModel;
using TTT.DataClasses.HexData;
using UnityEngine;

// references:
// https://onnxruntime.ai/docs/get-started/with-csharp.html
// https://github.com/asus4/onnxruntime-unity-examples/tree/main
//
namespace TTT.ClimateModel
{
    public sealed class MLModel
    {
        private static readonly string _MODEL_FILE_NAME = "xgboost_model.onnx";

        private static readonly string _MODEL_PATH;

        public static readonly double TRAINING_DATASET_GMSL_LOWER_BOUND =
            -1.3122;

        public static readonly double TRAINING_DATASET_GMSL_UPPER_BOUND =
            165.2076002;

        // private static readonly List<string> _modelInputFeatureNames = new()
        // {
        //     "CO2 (ppm)",
        //     "TEMP (deg C)",
        //     "Absolute GMSL (mm) relative to Jan 1950",
        //     "CO2_12m_ago",
        //     "CO2_5y_ago",
        //     "CO2_10y_ago",
        //     "TEMP_12m_ago",
        //     "TEMP_5y_ago",
        //     "TEMP_10y_ago",
        //     "GMSL_12m_ago",
        //     "GMSL_5y_ago",
        //     "GMSL_10y_ago",
        // };

        private static readonly int ONE_YEAR_WORLD_STATE_INDEX = 1;
        private static readonly int FIVE_YEAR_WORLD_STATE_INDEX = 5;
        private static readonly int TEN_YEAR_WORLD_STATE_INDEX = 10;

        static MLModel()
        {
            _MODEL_PATH = Path.Combine(
                Application.streamingAssetsPath,
                "MLModels",
                _MODEL_FILE_NAME
            );

            //todo: remove later
            if (!File.Exists(_MODEL_PATH))
            {
                Debug.LogError($"ONNX model not found at path: {_MODEL_PATH}");
            }
        }

        public double PredictFutureSeaLevel(
            ClimateModelInput input,
            ref Queue<WorldState> climateModelWorldStatesQueue
        )
        {
            using InferenceSession onnxInferenceSession = new(_MODEL_PATH);

            float[] modelInputFeatures = CreateModelInputList(
                input,
                ref climateModelWorldStatesQueue
            );

            // Adjust shape & input name to match your exported ONNX model
            var inputTensor = new DenseTensor<float>(
                modelInputFeatures,
                new[] { 1, modelInputFeatures.Length }
            );

            using var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor(_inputName, inputTensor),
            };
        }

        /// <summary>
        /// Creates a list of model inputs derived from the inputs from the Unity game.
        /// </summary>
        private static float[] CreateModelInputList(
            ClimateModelInput input,
            ref Queue<WorldState> climateModelWorldStatesQueue
        )
        {
            // 10y ago
            WorldState worldState10YearsAgo =
                climateModelWorldStatesQueue.ElementAt(
                    TEN_YEAR_WORLD_STATE_INDEX
                );

            // 5y ago
            WorldState worldState5YearsAgo =
                climateModelWorldStatesQueue.ElementAt(
                    FIVE_YEAR_WORLD_STATE_INDEX
                );

            // 12m ago
            WorldState worldState12MonthsAgo =
                climateModelWorldStatesQueue.ElementAt(
                    ONE_YEAR_WORLD_STATE_INDEX
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
    }
}
