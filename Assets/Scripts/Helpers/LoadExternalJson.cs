using System.IO;
using UnityEngine;
using WebSocketSharp;

namespace TTT.Helpers
{
    public static class LoadExternalJson
    {
        public static bool TryGetMapJson(
            string filePath,
            out TextAsset textAsset
        )
        {
            textAsset = new TextAsset();
            if (filePath.IsNullOrEmpty() || !filePath.EndsWith(".json"))
            {
                return false;
            }

            using StreamReader sr = new(filePath);
            textAsset = new TextAsset(sr.ReadToEnd());
            return true;
        }
    }
}
