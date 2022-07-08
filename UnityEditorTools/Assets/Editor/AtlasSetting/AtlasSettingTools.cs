using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class AtlasInfo
{
    public string atlasPath;
    public string atlasName;
    public TextureImporterFormat iosCompressType;
    public TextureImporterFormat androidCompressType;
    public bool focus;

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(atlasPath);
        sb.Append("|");
        sb.Append(atlasName);
        sb.Append("|");
        sb.Append((int) iosCompressType);
        sb.Append("|");
        sb.Append((int) androidCompressType);
        sb.Append("|");
        sb.Append(focus ? "1" : "0");
        return sb.ToString();
    }
}

public class AtlasSettingTools : EditorWindow
{
    private Vector3 scrollPos = Vector2.zero;
    private const string PATH_PREFIX = "Assets/Bundles/";
    private static AtlasSettingTools atlasSetting;
    private static string savePath;
    private static List<AtlasInfo> allAtlasInfos = new List<AtlasInfo>();

    [MenuItem("Tools/AtlasSetting/Setting")]
    private static void ShowWindow()
    {
        if (atlasSetting == null)
        {
            atlasSetting = GetWindow<AtlasSettingTools>();
        }
        else
        {
            atlasSetting.Close();
            atlasSetting = null;
        }
    }

    private static string GetCompressTxtPath()
    {
        if (string.IsNullOrEmpty(savePath))
        {
            savePath = Path.Combine(Application.dataPath, "../Tools/TextureCompress.txt");
        }

        return savePath;
    }

    public static AtlasInfo GetAtlasInfo(string dirPath)
    {
        if (allAtlasInfos.Count == 0)
        {
            SetAtlasInfo();
        }

        AtlasInfo infoTemp = allAtlasInfos.Find(x => { return x.atlasPath.Contains(dirPath); });
        return infoTemp;
    }

    private static void SetAtlasInfo()
    {
        allAtlasInfos.Clear();
        string[] lines = File.ReadAllLines(GetCompressTxtPath());
        foreach (string line in lines)
        {
            if (string.IsNullOrEmpty(line))
            {
                continue;
            }

            string[] infos = line.Split('|');
            AtlasInfo temp = new AtlasInfo();
            temp.atlasPath = infos[0];
            temp.atlasName = infos[1];
            int compressType = Int32.Parse(infos[2]);
            temp.iosCompressType = (TextureImporterFormat) compressType;
            compressType = Int32.Parse(infos[3]);
            temp.androidCompressType = (TextureImporterFormat) compressType;
            temp.focus = infos[4] == "1";
            allAtlasInfos.Add(temp);
        }
    }

    private void OnEnable()
    {
        SetAtlasInfo();
    }

    private void SaveToFile()
    {
        StringBuilder sb = new StringBuilder();
        foreach (var atlasInfo in allAtlasInfos)
        {
            sb.AppendLine(atlasInfo.ToString());
        }

        File.WriteAllText(GetCompressTxtPath(), sb.ToString());
    }

    private void OnGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Add", EditorStyles.toolbarButton))
        {
            allAtlasInfos.Add(new AtlasInfo());
        }

        if (GUILayout.Button("Setting Focus", EditorStyles.toolbarButton))
        {
            foreach (var atlasInfo in allAtlasInfos)
            {
                if (atlasInfo.focus)
                {
                    CompressTexture(atlasInfo);
                }
            }
        }

        if (GUILayout.Button("Setting All", EditorStyles.toolbarButton))
        {
            foreach (var atlasInfo in allAtlasInfos)
            {
                CompressTexture(atlasInfo);
            }
        }

        if (GUILayout.Button("Save", EditorStyles.toolbarButton))
        {
            SaveToFile();
        }

        GUILayout.EndHorizontal();
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        GUILayout.Label($"total count:{allAtlasInfos.Count}", GUILayout.MaxWidth(100));
        var colorTemp = GUI.color;
        for (var i = 0; i < allAtlasInfos.Count; i++)
        {
            GUILayout.BeginHorizontal();
            if (allAtlasInfos[i].focus)
            {
                GUI.color = Color.magenta;
            }

            allAtlasInfos[i].focus = GUILayout.Toggle(allAtlasInfos[i].focus, "", GUILayout.Width(15));

            if (GUILayout.Button("↑"))
            {
                if (i - 1 >= 0)
                {
                    var temp = allAtlasInfos[i - 1];
                    allAtlasInfos[i - 1] = allAtlasInfos[i];
                    allAtlasInfos[i] = temp;
                }
            }

            if (GUILayout.Button("↓"))
            {
                if (i + 1 < allAtlasInfos.Count)
                {
                    var temp = allAtlasInfos[i + 1];
                    allAtlasInfos[i + 1] = allAtlasInfos[i];
                    allAtlasInfos[i] = temp;
                }
            }

            Rect rect4 = EditorGUILayout.GetControlRect(GUILayout.MaxWidth(200));
            allAtlasInfos[i].atlasPath = EditorGUI.TextField(rect4, allAtlasInfos[i].atlasPath);
            if ((Event.current.type == EventType.DragExited) &&
                rect4.Contains(Event.current.mousePosition))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                if (DragAndDrop.paths != null && DragAndDrop.paths.Length > 0)
                {
                    string pathTemp = DragAndDrop.paths[0];
                    pathTemp = pathTemp.Replace(PATH_PREFIX, string.Empty);
                    var temp = allAtlasInfos.FindAll(x => { return x.atlasPath == pathTemp; });
                    if (temp.Count > 0)
                    {
                        EditorUtility.DisplayDialog("Add Error", "A related element already exists", "Confirm");
                    }
                    else
                    {
                        allAtlasInfos[i].atlasPath = pathTemp;
                    }
                }
            }

            GUILayout.Label("PackingTag:", GUILayout.MaxWidth(80));
            allAtlasInfos[i].atlasName = EditorGUILayout.TextField(allAtlasInfos[i].atlasName);

            GUILayout.Label("IosCompress:", GUILayout.MaxWidth(80));
            allAtlasInfos[i].iosCompressType =
                (TextureImporterFormat) EditorGUILayout.EnumPopup(allAtlasInfos[i].iosCompressType);

            GUILayout.Label("AndroidCompress:", GUILayout.MaxWidth(80));
            allAtlasInfos[i].androidCompressType =
                (TextureImporterFormat) EditorGUILayout.EnumPopup(allAtlasInfos[i].androidCompressType);
            if (GUILayout.Button("Setting"))
            {
                CompressTexture(allAtlasInfos[i]);
            }

            GUI.color = colorTemp;
            GUILayout.EndHorizontal();
        }

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndScrollView();
    }

    private void CompressTexture(AtlasInfo atlasInfo)
    {
        string pathInfo = PATH_PREFIX + atlasInfo.atlasPath;
        string[] files = Directory.GetFiles(pathInfo, "*.*", SearchOption.TopDirectoryOnly);
        for (var i = 0; i < files.Length; i++)
        {
            string pathTemp = files[i];
            pathTemp = pathTemp.Replace("\\", "/");
            if (pathTemp.EndsWith(".png") || pathTemp.EndsWith(".jpg") || pathTemp.EndsWith(".tga"))
            {
                TextureImporter textureImporter = AssetImporter.GetAtPath(pathTemp) as TextureImporter;
                textureImporter.spritePackingTag = atlasInfo.atlasName;
                int maxTextureSize = 0;
                TextureImporterFormat format;
                textureImporter.GetPlatformTextureSettings("Android", out maxTextureSize, out format);
                TextureImporterPlatformSettings androidSetting =
                    GetTextureImporterPlatformSettings("Android", atlasInfo.androidCompressType);
                androidSetting.maxTextureSize = maxTextureSize;
                textureImporter.SetPlatformTextureSettings(androidSetting);

                TextureImporterPlatformSettings iosSetting =
                    GetTextureImporterPlatformSettings("iPhone", atlasInfo.iosCompressType);
                iosSetting.maxTextureSize = maxTextureSize;
                textureImporter.SetPlatformTextureSettings(iosSetting);
                
                textureImporter.SaveAndReimport();
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private TextureImporterPlatformSettings GetTextureImporterPlatformSettings(string platformName,
        TextureImporterFormat formatType)
    {
        TextureImporterPlatformSettings textureImporterPlatformSettings = new TextureImporterPlatformSettings();
        textureImporterPlatformSettings.androidETC2FallbackOverride = AndroidETC2FallbackOverride.UseBuildSettings;
        textureImporterPlatformSettings.resizeAlgorithm = TextureResizeAlgorithm.Mitchell;
        textureImporterPlatformSettings.overridden = true;
        textureImporterPlatformSettings.compressionQuality = 50;
        textureImporterPlatformSettings.name = platformName;
        textureImporterPlatformSettings.format = formatType;
        return textureImporterPlatformSettings;
    }
}