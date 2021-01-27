using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;


public class BmfInfo
{
    public string filePath;
    public string fileName;
    public int outWidth = 256;
    public int outHeight = 256;
}

public class BMFontTools : EditorWindow
{
    private string configDirPath;

    private string configFlag = "# imported icon images";

    //字体文件生成路径
    private string fontDirPath;
    private Vector2 scrollPos;
    private string currentSelect = string.Empty;

    private struct FontInfo
    {
        //图片路径
        public string ImgPath;

        //字符
        public char charInfo;
    }

    private List<FontInfo> _fontInfoList = new List<FontInfo>();

    private readonly List<BmfInfo> bmfInfoList = new List<BmfInfo>();

    private void Awake()
    {
        configDirPath = Path.Combine(Application.dataPath, "../Tools/BMFont");
        fontDirPath = Path.Combine(Application.dataPath, "Bundles/Fonts");
        InitConfigInfo();
    }

    private void InitConfigInfo()
    {
        bmfInfoList.Clear();
        List<string> pathList = new List<string>(Directory.GetFiles(configDirPath, "*.bmfc"));
        for (int i = 0; i < pathList.Count; i++)
        {
            BmfInfo info = new BmfInfo();
            info.fileName = Path.GetFileNameWithoutExtension(pathList[i]);
            info.filePath = pathList[i];
            string[] tempLines = File.ReadAllLines(info.filePath);
            foreach (string tempLine in tempLines)
            {
                if (tempLine.StartsWith("outWidth="))
                {
                    string infoTemp = tempLine.Split('#')[0];
                    int width = int.Parse(infoTemp.Replace("outWidth=", string.Empty));
                    info.outWidth = width;
                }
                else if (tempLine.StartsWith("outHeight="))
                {
                    string infoTemp = tempLine.Split('#')[0];
                    int height = int.Parse(infoTemp.Replace("outHeight=", string.Empty));
                    info.outHeight = height;
                }
            }

            bmfInfoList.Add(info);
        }

        if (!bmfInfoList.IsNullOrEmpty())
        {
            currentSelect = bmfInfoList[0].filePath;
            _fontInfoList = AnalysisConfig(currentSelect);
        }
    }

#if UNITY_EDITOR_WIN
    [MenuItem("Tools/BMFontTools", false, 50)]
    private static void MyBMFontTools()
    {
        BMFontTools bmFont = GetWindow<BMFontTools>();
        bmFont.Show();
    }
#endif

    private void OnGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Refresh", EditorStyles.toolbarButton))
        {
            InitConfigInfo();
        }

        if (GUILayout.Button("NewFont", EditorStyles.toolbarButton))
        {
            string fileName = $"{DateTime.Now:yyyyMMddhhmmss}.bmfc";
            File.WriteAllText(Path.Combine(configDirPath, fileName), configFlag, Encoding.UTF8);
            InitConfigInfo();
            currentSelect = bmfInfoList.ToFind(x => x.filePath.Contains(fileName)).filePath;
            _fontInfoList = AnalysisConfig(currentSelect);
        }

        GUILayout.EndHorizontal();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Font Save Path:", GUILayout.MaxWidth(100));
        GUILayout.TextField(fontDirPath);
        EditorGUILayout.EndHorizontal();
        SetBMFontConfigs();
        SetConfigInfo();

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();
    }

    private void SetBMFontConfigs()
    {
        for (int i = 0; i < bmfInfoList.Count; i++)
        {
            if (bmfInfoList[i].filePath.Equals(currentSelect))
            {
                GUI.color = Color.green;
            }

            string filePath = bmfInfoList[i].filePath;

            GUILayout.BeginHorizontal();
            GUI.enabled = false;
            EditorGUILayout.TextField(filePath, GUILayout.MaxWidth(500));
            GUI.enabled = true;
            EditorGUILayout.LabelField("FontName:", GUILayout.MaxWidth(80));
            bmfInfoList[i].fileName = EditorGUILayout.TextField(bmfInfoList[i].fileName, GUILayout.MaxWidth(100));
            EditorGUILayout.LabelField("outWidth:", GUILayout.MaxWidth(70));
            bmfInfoList[i].outWidth =
                int.Parse(EditorGUILayout.TextField(bmfInfoList[i].outWidth.ToString(), GUILayout.MaxWidth(50)));
            EditorGUILayout.LabelField("outHeight:", GUILayout.MaxWidth(70));
            bmfInfoList[i].outHeight =
                int.Parse(EditorGUILayout.TextField(bmfInfoList[i].outHeight.ToString(), GUILayout.MaxWidth(50)));
            if (GUILayout.Button("ReName"))
            {
                Regex regex = new Regex(@"/|\\|<|>|\*|\?");
                if (!string.IsNullOrEmpty(bmfInfoList[i].fileName) && !regex.IsMatch(bmfInfoList[i].fileName))
                {
                    string fileNameTemp = filePath.Replace(Path.GetFileNameWithoutExtension(filePath),
                        bmfInfoList[i].fileName);
                    if (File.Exists(fileNameTemp))
                    {
                        Debug.LogError("文件冲突，命名失败");
                    }
                    else
                    {
                        File.Move(filePath, fileNameTemp);
                    }

                    InitConfigInfo();
                }
                else
                {
                    Debug.LogError("文件名非法或为空，命名失败");
                }
            }

            if (GUILayout.Button("Select"))
            {
                currentSelect = filePath;
                _fontInfoList = AnalysisConfig(currentSelect);
            }

            GUILayout.EndHorizontal();
            if (filePath.Equals(currentSelect))
            {
                GUI.color = Color.white;
            }
        }
    }

    private void SetConfigInfo()
    {
        if (!string.IsNullOrEmpty(currentSelect))
        {
            for (int i = 0; i < _fontInfoList.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Select Img", GUILayout.MaxWidth(100)))
                {
                    string pathTemp = EditorUtility.OpenFilePanelWithFilters("选择图片", "", new[] {"Image", "png"});
                    if (!string.IsNullOrEmpty(pathTemp))
                    {
                        FontInfo fontInfo = new FontInfo();
                        fontInfo.charInfo = _fontInfoList[i].charInfo;
                        fontInfo.ImgPath = FormatPath(pathTemp);
                        _fontInfoList[i] = fontInfo;
                    }
                }

                EditorGUILayout.LabelField("Char:", GUILayout.MaxWidth(55));

                FontInfo info = new FontInfo();
                if (!string.IsNullOrEmpty(_fontInfoList[i].charInfo.ToString()))
                {
                    string temp =
                        EditorGUILayout.TextField(_fontInfoList[i].charInfo.ToString(), GUILayout.MaxWidth(30));
                    if (temp.Length == 1 && Regex.IsMatch(temp, "[\x20-\x7e]"))
                    {
                        info.charInfo = temp[0];
                        info.ImgPath = _fontInfoList[i].ImgPath;
                        _fontInfoList[i] = info;
                    }
                }

                EditorGUILayout.LabelField("ImgPath:", GUILayout.MaxWidth(55));
                GUI.enabled = false;
                EditorGUILayout.TextField(_fontInfoList[i].ImgPath);
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Add"))
            {
                _fontInfoList.Add(new FontInfo());
            }

            GUI.enabled = !_fontInfoList.IsNullOrEmpty();
            if (GUILayout.Button("Save And Export Font"))
            {
                SaveFontAndExport();
            }

            GUI.enabled = true;
        }
    }

    private void SaveFontAndExport()
    {
        BmfInfo bmfInfo = bmfInfoList.ToFind(x => x.filePath.Equals(currentSelect));
        string baseFontInfo = File.ReadAllText(configDirPath + "/BaseConfig.bmf");
        baseFontInfo = baseFontInfo
            .Replace("outWidth=", $"outWidth={bmfInfo.outWidth}#")
            .Replace("outHeight=", $"outHeight={bmfInfo.outHeight}#");
        baseFontInfo += "\n";
        for (int i = 0; i < _fontInfoList.Count; i++)
        {
            if (string.IsNullOrEmpty(_fontInfoList[i].ImgPath) ||
                string.IsNullOrEmpty(_fontInfoList[i].charInfo.ToString()))
            {
                continue;
            }

            string info = $"icon=\"{_fontInfoList[i].ImgPath}\",{(int) _fontInfoList[i].charInfo},0,0,0\n";
            baseFontInfo += info;
        }

        File.WriteAllText(currentSelect, baseFontInfo);

        ExportFontInfo();

        AssetDatabase.Refresh();
    }

    private void ExportFontInfo()
    {
        string fileName = Path.GetFileNameWithoutExtension(currentSelect);
        string targetDir = Path.Combine(fontDirPath, fileName);
        if (!Directory.Exists(targetDir))
        {
            Directory.CreateDirectory(targetDir);
        }

        Process process = new Process();
        string batPath = Path.Combine(configDirPath, "BMFontGenerate.bat");
        process.StartInfo.FileName = batPath;
        process.StartInfo.WorkingDirectory = configDirPath;
        process.StartInfo.Arguments =
            string.Format("{0} {1}", currentSelect, Path.Combine(fontDirPath, targetDir + "/" + fileName));
        process.Start();
        process.WaitForExit();
        AssetDatabase.Refresh();
        GenFontInfo(targetDir, fileName);
    }

    private string GetAssetPath(string path)
    {
        string pathTemp = path.Replace("\\", "/");
        pathTemp = pathTemp.Replace(Application.dataPath, "Assets");
        return pathTemp;
    }

    private void GenFontInfo(string fontDirPath, string fileName)
    {
        string matPath = Path.Combine(fontDirPath, fileName + "_0.mat");
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(GetAssetPath(matPath));
        if (mat == null)
        {
            mat = new Material(Shader.Find("UI/Default Font"));
            AssetDatabase.CreateAsset(mat, GetAssetPath(matPath));
        }

        string texturePath = Path.Combine(fontDirPath, fileName + "_0.png");
        Texture _fontTexture = AssetDatabase.LoadAssetAtPath<Texture>(GetAssetPath(texturePath));
        mat = AssetDatabase.LoadAssetAtPath<Material>(GetAssetPath(matPath));
        mat.SetTexture("_MainTex", _fontTexture);

        string fontPath = Path.Combine(fontDirPath, fileName + ".fontsettings");
        Font font = AssetDatabase.LoadAssetAtPath<Font>(GetAssetPath(fontPath));
        if (font == null)
        {
            font = new Font();
            AssetDatabase.CreateAsset(font, GetAssetPath(fontPath));
        }

        string fontConfigPath = Path.Combine(fontDirPath, fileName + ".fnt");
        List<CharacterInfo> chars = GetFontInfo(fontConfigPath, _fontTexture);
        font.material = mat;
        SerializeFont(font, chars, 1);
        File.Delete(fontConfigPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private List<CharacterInfo> GetFontInfo(string fontConfig, Texture texture)
    {
        XmlDocument xml = new XmlDocument();
        xml.Load(fontConfig);
        List<CharacterInfo> chtInfoList = new List<CharacterInfo>();
        XmlNode node = xml.SelectSingleNode("font/chars");
        foreach (XmlNode nd in node.ChildNodes)
        {
            XmlElement xe = (XmlElement) nd;
            int x = int.Parse(xe.GetAttribute("x"));
            int y = int.Parse(xe.GetAttribute("y"));
            int width = int.Parse(xe.GetAttribute("width"));
            int height = int.Parse(xe.GetAttribute("height"));
            int advance = int.Parse(xe.GetAttribute("xadvance"));
            CharacterInfo info = new CharacterInfo();
            info.glyphHeight = texture.height;
            info.glyphWidth = texture.width;
            info.index = int.Parse(xe.GetAttribute("id"));
            //这里注意下UV坐标系和从BMFont里得到的信息的坐标系是不一样的哦，前者左下角为（0,0），
            //右上角为（1,1）。而后者则是左上角上角为（0,0），右下角为（图宽，图高）
            info.uvTopLeft = new Vector2((float) x / texture.width, 1 - (float) y / texture.height);
            info.uvTopRight = new Vector2((float) (x + width) / texture.width, 1 - (float) y / texture.height);
            info.uvBottomLeft = new Vector2((float) x / texture.width, 1 - (float) (y + height) / texture.height);
            info.uvBottomRight = new Vector2((float) (x + width) / texture.width,
                1 - (float) (y + height) / texture.height);

            info.minX = 0;
            info.minY = -height;
            info.maxX = width;
            info.maxY = 0;

            info.advance = advance;

            chtInfoList.Add(info);
        }

        return chtInfoList;
    }

    private static void SetLineHeight(SerializedObject font, float height)
    {
        font.FindProperty("m_LineSpacing").floatValue = height;
    }

    private static SerializedObject SerializeFont(Font font, List<CharacterInfo> chars, float lineHeight)
    {
        SerializedObject serializedFont = new SerializedObject(font);
        SetLineHeight(serializedFont, lineHeight);
        SerializeFontCharInfos(serializedFont, chars);
        serializedFont.ApplyModifiedProperties();
        return serializedFont;
    }

    private static void SerializeFontCharInfos(SerializedObject font, List<CharacterInfo> chars)
    {
        SerializedProperty charRects = font.FindProperty("m_CharacterRects");
        charRects.arraySize = chars.Count;
        for (int i = 0; i < chars.Count; ++i)
        {
            CharacterInfo info = chars[i];
            SerializedProperty prop = charRects.GetArrayElementAtIndex(i);
            SerializeCharInfo(prop, info);
        }
    }

    private static void SerializeCharInfo(SerializedProperty prop, CharacterInfo charInfo)
    {
        prop.FindPropertyRelative("index").intValue = charInfo.index;
        prop.FindPropertyRelative("uv").rectValue = charInfo.uv;
        prop.FindPropertyRelative("vert").rectValue = charInfo.vert;
        prop.FindPropertyRelative("advance").floatValue = charInfo.advance;
        prop.FindPropertyRelative("flipped").boolValue = false;
    }

    private List<FontInfo> AnalysisConfig(string configPath)
    {
        List<FontInfo> infoList = new List<FontInfo>();
        string[] fileInfo = File.ReadAllLines(configPath);
        bool isGetInfoFlag = false;
        for (int i = 0; i < fileInfo.Length; i++)
        {
            if (fileInfo[i].Contains(configFlag) || isGetInfoFlag)
            {
                if (!isGetInfoFlag)
                {
                    i++;
                    isGetInfoFlag = true;
                }

                if (i < fileInfo.Length && !string.IsNullOrEmpty(fileInfo[i]))
                {
                    infoList.Add(GetFontInfoByStr(fileInfo[i]));
                }
            }
        }

        return infoList;
    }

    private FontInfo GetFontInfoByStr(string str)
    {
        string[] strTemp = str.Split(',');
        FontInfo fontInfo = new FontInfo();
        string strPathTemp = string.Empty;
        for (int i = 0; i < strTemp.Length; i++)
        {
            if (IsOddDoubleQuota(strTemp[i]))
            {
                strPathTemp += strTemp[i] + ",";
                if (!IsOddDoubleQuota(strPathTemp))
                {
                    strPathTemp = strPathTemp.Substring(0, strPathTemp.Length - 1);
                    break;
                }
            }
            else
            {
                strPathTemp = strTemp[i];
                break;
            }
        }

        fontInfo.ImgPath = strPathTemp.Replace("icon=\"", string.Empty).Replace("\"", string.Empty);
        fontInfo.charInfo = (char) int.Parse(strTemp[strTemp.Length - 4]);
        return fontInfo;
    }

    private bool IsOddDoubleQuota(string str)
    {
        return GetDoubleQuotaCount(str) % 2 == 1;
    }

    private int GetDoubleQuotaCount(string str)
    {
        string[] strArray = str.Split('"');
        int doubleQuotaCount = strArray.Length - 1;
        doubleQuotaCount = doubleQuotaCount < 0 ? 0 : doubleQuotaCount;
        return doubleQuotaCount;
    }

    private string FormatPath(string path)
    {
        path = path.Replace("\\", "/");
        return path;
    }
}