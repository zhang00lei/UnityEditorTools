using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SpinResImportSetting : UnityEditor.AssetPostprocessor
{
    private const string SPIN_RES_DIR = "/SpinRes/";

    private static Dictionary<string, string> replaceInfoDict = new Dictionary<string, string>()
    {
        {".atlas", ".txt"},
        {".skel",".bytes"},
    };

    public static void OnPostprocessAllAssets(string[] importedAsset, string[] deletedAssets, string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        foreach (string str in importedAsset)
        {
            if (!str.Contains(SPIN_RES_DIR))
            {
                continue;
            }

            string dataPath = Application.dataPath.Replace("Assets", string.Empty);
            foreach (var info in replaceInfoDict)
            {
                if (str.EndsWith(info.Key))
                {
                    string sourcePath = Path.Combine(dataPath, str);
                    string toPath = Path.Combine(dataPath, str + info.Value);
                    File.Move(sourcePath, toPath);
                }
            }
        }
    }
}