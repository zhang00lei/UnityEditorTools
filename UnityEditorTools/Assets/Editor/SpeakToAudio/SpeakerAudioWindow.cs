using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = System.Random;

public class SpeakerAudioWindow : EditorWindow
{
    public enum VoiceType
    {
        Xiaoxiao,
        Yunyang,
    }

    public struct VoiceInfo
    {
        public string voiceStr;
        public VoiceType voiceType;
    }

    [MenuItem("Tools/SpeakerAudio")]
    private static void OpenWindow()
    {
        GetWindow<SpeakerAudioWindow>();
    }

    private string speakStr;
    private VoiceType voiceType = VoiceType.Xiaoxiao;
    private string audioPath;

    private void OnEnable()
    {
        audioPath = Path.Combine(Application.dataPath, "test.mp3");
    }

    private void OnGUI()
    {
        voiceType = (VoiceType)EditorGUILayout.EnumPopup("请选择音频类型:", voiceType);

        EditorGUILayout.LabelField("请输入文字:");
        speakStr = EditorGUILayout.TextArea(speakStr, GUILayout.Height(100));
        audioPath = EditorGUILayout.TextField("生成文件路径:", audioPath);
        if (GUILayout.Button("生成"))
        {
            ToAudio(speakStr, audioPath, voiceType);
        }
    }

    public static void ToAudio(string voiceStr, string audioPath, VoiceType voiceType)
    {
        voiceStr = voiceStr.Replace("\n", "。");
        string cmdStr =
            $"/c chcp 437&&aspeak -t \"{voiceStr}\" -v zh-CN-{voiceType}Neural -o {audioPath}.mp3 --mp3 -q=3&exit";
        Debug.Log(cmdStr);
        Cmd(cmdStr);
        AssetDatabase.Refresh();
    }

    private static void Cmd(string cmdStr)
    {
        Process p = new Process();
        p.StartInfo.FileName = "cmd.exe";
        p.StartInfo.Arguments = cmdStr;
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.RedirectStandardInput = true;
        p.StartInfo.RedirectStandardError = true;
        p.StartInfo.CreateNoWindow = true;
        p.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                Debug.Log(e.Data);
            }
        };
        p.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                Debug.LogError(e.Data);
            }
        };
        p.Start();
        p.BeginOutputReadLine();
        p.WaitForExit();
        p.Close();
        p.Dispose();
    }
}