using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class GitLogInfo
{
    public readonly string LogId;
    public readonly string Author;
    public readonly string LogInfo;
    public readonly string PullDate;

    public GitLogInfo(string logId, string author, string logInfo, string pullDate)
    {
        LogId = logId;
        Author = author;
        LogInfo = logInfo;
        PullDate = pullDate;
    }
}

public class GitLog
{
    private static readonly List<GitLogInfo> gitLogInfoList = new List<GitLogInfo>();

    public static void GitCommand(string commandStr, DataReceivedEventHandler dataReceivedEvent)
    {
#if UNITY_EDITOR_WIN
        string gitPath = @"D:\Program Files\Git\bin\git.exe";
#else
            string gitPath = "git";
#endif
        Process p = new Process();
        p.StartInfo.FileName = gitPath;
        p.StartInfo.Arguments = commandStr;
        p.StartInfo.CreateNoWindow = true;
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.WorkingDirectory = Application.dataPath;
        p.OutputDataReceived += dataReceivedEvent;
        p.Start();
        p.BeginOutputReadLine();
        p.WaitForExit();
        p.Close();
        p.Dispose();
    }

    public static string GetGitLog(bool isGetAll = false)
    {
        StringBuilder sb = new StringBuilder();

        gitLogInfoList.Clear();
        GitCommand("log -200 --date=format:\"%Y-%m-%d %H:%M:%S\" --pretty=format:%H|%an|%s|%cd", OnGitLogOutput);

        gitLogInfoList.Sort((x, y) => { return String.Compare(y.PullDate, x.PullDate, StringComparison.Ordinal); });

        for (int i = 0; i < gitLogInfoList.Count; i++)
        {
            GitLogInfo gitLogInfo = gitLogInfoList[i];
            sb.Append(i + 1)
                .Append(".")
                .Append(gitLogInfo.Author)
                .Append("\t")
                .Append(gitLogInfo.PullDate)
                .Append("\t")
                .Append(gitLogInfo.LogInfo)
                .Append("\n");
        }

        string result = sb.ToString();
        return result;
    }

    private static void OnGitLogOutput(object sender, DataReceivedEventArgs e)
    {
        if (e != null && !string.IsNullOrEmpty(e.Data))
        {
            string[] infos = e.Data.Split('|');
            string logInfo = infos[2];


            if (gitLogInfoList.Count >= 30)
            {
                return;
            }

            GitLogInfo gitLogInfo = new GitLogInfo(infos[0], infos[1], logInfo, infos[3]);
            gitLogInfoList.Add(gitLogInfo);
        }
    }

    [MenuItem("TortoiseGit/GitLog")]
    public static void OpenGitLog()
    {
        string info = GetGitLog(true);
        Debug.Log(info);
    }
}