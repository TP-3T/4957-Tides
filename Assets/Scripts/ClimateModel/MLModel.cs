using System.IO;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using TTT.DataClasses.ClimateModel;
using UnityEngine;

// references:
// https://onnxruntime.ai/docs/get-started/with-csharp.html
// https://github.com/asus4/onnxruntime-unity-examples/tree/main
//
namespace TTT.ClimateModel
{
    public sealed class MLModel
    {
        private readonly InferenceSession _onnxInferenceSession;
        private static readonly string _MODEL_FILE_NAME = "xgboost_model.onnx";

        private static readonly string _MODEL_PATH;

        public static readonly double TRAINING_DATASET_GMSL_LOWER_BOUND =
            -1.3122;

        public static readonly double TRAINING_DATASET_GMSL_UPPER_BOUND =
            165.2076002;

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

        public MLModel()
        {
            _onnxInferenceSession = new InferenceSession(_MODEL_PATH);
        }

        public double PredictFutureSeaLevel(ClimateModelInput input)
        {
            //             {
            //     "CO2 (ppm)": "f0",
            //     "TEMP (deg C)": "f1",
            //     "Absolute GMSL (mm) relative to Jan 1950": "f2",
            //     "CO2_3m_ago": "f3",
            //     "CO2_6m_ago": "f4",
            //     "CO2_12m_ago": "f5",
            //     "CO2_5y_ago": "f6",
            //     "CO2_10y_ago": "f7",
            //     "TEMP_3m_ago": "f8",
            //     "TEMP_6m_ago": "f9",
            //     "TEMP_12m_ago": "f10",
            //     "TEMP_5y_ago": "f11",
            //     "TEMP_10y_ago": "f12",
            //     "GMSL_3m_ago": "f13",
            //     "GMSL_6m_ago": "f14",
            //     "GMSL_12m_ago": "f15",
            //     "GMSL_5y_ago": "f16",
            //     "GMSL_10y_ago": "f17"
            // }
        }
    }
}
