using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class ExcelTools : EditorWindow
{
    private List<string> xlsxPathList = new List<string>();
    private static string xlsxFolder;
    private static string luaFolder;
    private Vector2 scrollPos = Vector2.zero;

    private static void InitPath()
    {
        xlsxFolder = Path.Combine(Application.dataPath, "../Tools/ExcelToLua/Excels");
        luaFolder = Path.Combine(Application.dataPath, "LuaConfig");
    }

    [MenuItem("Tools/ExcelToLua")]
    private static void Init()
    {
        InitPath();
        GetWindow(typeof(ExcelTools));
    }

    private void OnEnable()
    {
        RefreshExcelInfo();
    }

    private void OnGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("GenLuaConfig", EditorStyles.toolbarButton))
        {
            InitPath();
            XlsxGenLua();
        }

        if (GUILayout.Button("Refresh", EditorStyles.toolbarButton))
        {
            InitPath();
            RefreshExcelInfo();
        }

        GUILayout.EndHorizontal();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        GUILayout.BeginVertical();
        DrawExcelInfo();
        GUILayout.EndVertical();
        EditorGUILayout.EndScrollView();
    }

    private void DrawExcelInfo()
    {
        foreach (string pathInfo in xlsxPathList)
        {
            GUILayout.Space(3);
            GUILayout.BeginHorizontal();
            string excelName = pathInfo.Split('/').Last().Split('.').First();
            if (GUILayout.Button($"Open({excelName})", GUILayout.MaxWidth(500)))
            {
                Process.Start(pathInfo);
            }

            GUILayout.EndHorizontal();
        }
    }


    private void RefreshExcelInfo()
    {
        xlsxPathList.Clear();
        if (!Directory.Exists(xlsxFolder))
        {
            Debug.LogError("excel路径不存在");
            return;
        }

        string[] pathArray = Directory.GetFiles(xlsxFolder);
        foreach (string pathInfo in pathArray)
        {
            xlsxPathList.Add(pathInfo.Replace("\\", "/"));
        }
    }


    private void XlsxGenLua()
    {
        string[] files = Directory.GetFiles(xlsxFolder);
        foreach (var item in files)
        {
            string fileName = Path.GetFileNameWithoutExtension(item);

            Process p = new Process();
            p.StartInfo.FileName = Path.Combine(Application.dataPath, "../Tools/Python3.9/python.exe");
            p.StartInfo.Arguments =
                string.Format("excel2lua.py Excels/{0}.xlsx {1}/{2}.lua", fileName, luaFolder, fileName);
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.WorkingDirectory = xlsxFolder + "/..";
            p.Start();
            p.BeginOutputReadLine();
            p.OutputDataReceived += new DataReceivedEventHandler((object sender, DataReceivedEventArgs e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    Debug.Log(e.Data);
                }
            });
        }

        AssetDatabase.Refresh();
    }
}