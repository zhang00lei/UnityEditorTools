using System.IO;
using UnityEditor;
using UnityEngine;

public class MetaAssetImporter : AssetPostprocessor
{
    public static void OnPostprocessAllAssets(string[] importedAsset, string[] deletedAssets, string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        foreach (string str in importedAsset)
        {
            string filePath = str.Substring(7);
            string[] fileInfoTemp = filePath.Split('/');
            string fileName = fileInfoTemp[fileInfoTemp.Length - 1];

            string pinYin = PinYin.MakeSpellCode(fileName, SpellOptions.EnableUnicodeLetter);
            if (!string.Equals(fileName, pinYin))
            {
                var sourceFile = Path.Combine(Application.dataPath, filePath);
                var toFile = sourceFile.Replace(fileName, pinYin);
                File.Move(sourceFile, toFile);
            }
        }
    }
}