using System;
using System.Reflection;
using UnityEditor;

public class CTools
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
}