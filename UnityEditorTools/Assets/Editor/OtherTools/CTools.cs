using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

public static class CTools
{
    [MenuItem("Tools/RunOrStop _F5")]
    private static void StartAPP()
    {
        EditorApplication.isPlaying = !EditorApplication.isPlaying;
    }

    [MenuItem("Tools/ClearConsole _F1")]
    private static void ClearConsole()
    {
        Assembly assembly = Assembly.GetAssembly(typeof(ActiveEditorTracker));
        Type type = assembly.GetType("UnityEditorInternal.LogEntries");
        if (type == null)
        {
            type = assembly.GetType("UnityEditor.LogEntries");
        }

        MethodInfo method = type.GetMethod("Clear");
        method.Invoke(new object(), null);
    }
    
    [MenuItem("Assets/ExportAnimFromFBX")]
    private static void GetFiltered()
    {
        UnityEngine.Object[] objects = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
        foreach (UnityEngine.Object asset in objects)
        {
            if (!AssetDatabase.GetAssetPath(asset).ToUpper().EndsWith(".FBX"))
            {
                continue;
            }

            UnityEngine.Object[] assetTemp = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(asset));
            for (var i = 0; i < assetTemp.Length; i++)
            {
                if (!(assetTemp[i] is AnimationClip))
                {
                    continue;
                }

                AnimationClip fbxAnim = assetTemp[i] as AnimationClip;
                AnimationClip animationClip = new AnimationClip();
                EditorUtility.CopySerialized(fbxAnim, animationClip);
                string path = AssetDatabase.GetAssetPath(asset);
                path = Path.GetDirectoryName(path);
                AssetDatabase.CreateAsset(animationClip, Path.Combine(path, $"{fbxAnim.name}.anim"));
            }
        }

        AssetDatabase.Refresh();
    }
}