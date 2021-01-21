using UnityEditor;
using UnityEngine;

public class TortoiseEditor
{
    public static string tortoiseGitPath = @"D:\Program Files\TortoiseGit\bin\TortoiseGitProc.exe";

    [MenuItem("TortoiseGit/Assets/StashSave")]
    public static void GitAssetsStushSave()
    {
        TortoiseGit.GitCommand(GitType.StashSave, Application.dataPath, tortoiseGitPath);
    }

    [MenuItem("TortoiseGit/Assets/StashPop")]
    public static void GitAssetsStushPop()
    {
        TortoiseGit.GitCommand(GitType.StashPop, Application.dataPath, tortoiseGitPath);
    }

    [MenuItem("TortoiseGit/Assets/Push")]
    public static void GitAssetPush()
    {
        TortoiseGit.GitCommand(GitType.Push, Application.dataPath, tortoiseGitPath);
    }

    [MenuItem("TortoiseGit/Assets/Log _F9")]
    public static void GitAssetsLog()
    {
        string[] strs = Selection.assetGUIDs;
        if (strs.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(strs[0]);
            TortoiseGit.GitCommand(GitType.Log, path, tortoiseGitPath);
        }
        else
        {
            TortoiseGit.GitCommand(GitType.Log, Application.dataPath, tortoiseGitPath);
        }
    }

    [MenuItem("TortoiseGit/Assets/Pull _F10")]
    public static void GitAssetsPull()
    {
        TortoiseGit.GitCommand(GitType.Pull, Application.dataPath, tortoiseGitPath);
    }

    [MenuItem("TortoiseGit/Assets/Commit _F11")]
    public static void GitAssetsCommit()
    {
        TortoiseGit.GitCommand(GitType.Commit, Application.dataPath, tortoiseGitPath);
    }

    [MenuItem("TortoiseGit/ProjectSettings/Log")]
    public static void GitProjectSettingsLog()
    {
        TortoiseGit.GitCommand(GitType.Log, Application.dataPath + "/../ProjectSettings", tortoiseGitPath);
    }

    [MenuItem("TortoiseGit/ProjectSettings/Pull")]
    public static void GitProjectSettingsPull()
    {
        TortoiseGit.GitCommand(GitType.Pull, Application.dataPath + "/../ProjectSettings", tortoiseGitPath);
    }

    [MenuItem("TortoiseGit/ProjectSettings/Commit")]
    public static void GitProjectSettingsCommit()
    {
        TortoiseGit.GitCommand(GitType.Commit, Application.dataPath + "/../ProjectSettings", tortoiseGitPath);
    }
}