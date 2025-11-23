using System.IO;
using SFB;
using UnityEngine;

namespace TTT.Helpers
{
    public static class LoadExternalJson
    {
        private static readonly SFB.ExtensionFilter[] extensions = new[]
        {
            new SFB.ExtensionFilter("Data File", new string[] { "json" }),
        };

        public static bool TryGetDataJson(out TextAsset textAsset)
        {
            textAsset = new TextAsset();
            string[] paths = SelectFile();
            if (paths.Length == 0)
            {
                return false;
            }

            using StreamReader sr = new(paths[0]);
            textAsset = new TextAsset(sr.ReadToEnd());
            return true;
        }

        private static string[] SelectFile()
        {
            var paths = StandaloneFileBrowser.OpenFilePanel(
                "Load Data",
                "",
                extensions,
                false
            );
            return paths;
        }
    }
}
