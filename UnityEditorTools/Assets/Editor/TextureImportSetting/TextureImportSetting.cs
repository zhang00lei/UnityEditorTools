using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class TextureImportSetting : AssetPostprocessor
{
    private const string AutoSetImport = "AutoSetImport";

    private static readonly int[] MaxSizes = {32, 64, 128, 256, 512, 1024, 2048, 4096, 8192};

    private struct TextureImporterInfo
    {
        public readonly TextureImporterFormat TextureImporterFormat;
        public readonly string PackingTag;

        public TextureImporterInfo(TextureImporterFormat formatInfo, string packingTag)
        {
            TextureImporterFormat = formatInfo;
            PackingTag = packingTag;
        }
    }

    /// <summary>
    /// 设置对应目录下的贴图压缩格式、图集名称
    /// </summary>
    private readonly Dictionary<string, TextureImporterInfo> _textureImporterFormatsDict =
        new Dictionary<string, TextureImporterInfo>
        {
            {"Game", new TextureImporterInfo(TextureImporterFormat.ASTC_RGBA_6x6, "Game")},
            {"Comm", new TextureImporterInfo(TextureImporterFormat.ASTC_RGBA_6x6, "Comm")},
        };

    private static readonly string DEFAULTS_KEY = "DEFAULTS_DONE";
    private static readonly uint DEFAULTS_VERSION = 2;

    private bool IsAssetProcessed
    {
        get
        {
            string key = $"{DEFAULTS_KEY}_{DEFAULTS_VERSION}";
            return assetImporter.userData.Contains(key);
        }
        set
        {
            string key = $"{DEFAULTS_KEY}_{DEFAULTS_VERSION}";
            assetImporter.userData = value ? key : string.Empty;
        }
    }

    private void OnPreprocessTexture()
    {
        bool autoSetImport = EditorPrefs.GetBool(AutoSetImport, false);
        if (!autoSetImport)
        {
            return;
        }

        if (IsAssetProcessed)
        {
            return;
        }

        IsAssetProcessed = true;

        if (!assetPath.EndsWith(".jpg") && !assetPath.EndsWith(".png"))
        {
            return;
        }

        if (assetPath.IndexOf("AssetsPackage", StringComparison.Ordinal) < 0)
        {
            return;
        }

        TextureImporter textureImporter = (TextureImporter) assetImporter;
        if (textureImporter == null)
        {
            return;
        }

        textureImporter.textureType = TextureImporterType.Sprite;
        GetOriginalSize(textureImporter, out var width, out var height);

        bool isPowerOfTwo = GetIsPowerOfTwo(width) && GetIsPowerOfTwo(height);
        if (!isPowerOfTwo)
        {
            textureImporter.npotScale = TextureImporterNPOTScale.None;
        }

        textureImporter.mipmapEnabled = false;
        int maxTextureSize = GetMaxSize(Mathf.Max(width, height));
        string dirName = new DirectoryInfo(Path.GetDirectoryName(assetPath)).Name;

        TextureImporterPlatformSettings textureImporterPlatformSettings =
            GetTextureImporterPlatformSettings(maxTextureSize, dirName);
        textureImporter.SetPlatformTextureSettings(textureImporterPlatformSettings);
        if (_textureImporterFormatsDict.ContainsKey(dirName))
        {
            textureImporter.spritePackingTag = _textureImporterFormatsDict[dirName].PackingTag;
        }

        textureImporter.SaveAndReimport();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private TextureImporterPlatformSettings GetTextureImporterPlatformSettings(int size, string dirName)
    {
        TextureImporterPlatformSettings textureImporterPlatformSettings = new TextureImporterPlatformSettings();
        textureImporterPlatformSettings.androidETC2FallbackOverride = AndroidETC2FallbackOverride.UseBuildSettings;
        textureImporterPlatformSettings.maxTextureSize = GetMaxSize(size);
        textureImporterPlatformSettings.resizeAlgorithm = TextureResizeAlgorithm.Mitchell;
        textureImporterPlatformSettings.overridden = true;
        textureImporterPlatformSettings.compressionQuality = 50;
#if UNITY_ANDROID
        textureImporterPlatformSettings.name = "Android";
#elif UNITY_IOS
            textureImporterPlatformSettings.name = "iPhone";
#endif
        TextureImporterFormat formatType = TextureImporterFormat.ASTC_RGBA_6x6;
        if (_textureImporterFormatsDict.ContainsKey(dirName))
        {
            formatType = _textureImporterFormatsDict[dirName].TextureImporterFormat;
        }

        textureImporterPlatformSettings.format = formatType;

        return textureImporterPlatformSettings;
    }

    private void GetOriginalSize(TextureImporter importer, out int width, out int height)
    {
        object[] args = {0, 0};
        MethodInfo mi =
            typeof(TextureImporter).GetMethod("GetWidthAndHeight", BindingFlags.NonPublic | BindingFlags.Instance);
        mi.Invoke(importer, args);
        width = (int) args[0];
        height = (int) args[1];
    }

    private bool GetIsPowerOfTwo(int f)
    {
        return (f & (f - 1)) == 0;
    }

    private int GetMaxSize(int longerSize)
    {
        int index = 0;
        for (int i = 0; i < MaxSizes.Length; i++)
        {
            if (longerSize <= MaxSizes[i])
            {
                index = i;
                break;
            }
        }

        return MaxSizes[index];
    }

#if UNITY_IOS
        [MenuItem("Assets/FormatIosTexture")]
        public static void FormatAllIosTexture()
        {
            Texture2D[] textures = Selection.GetFiltered<Texture2D>(SelectionMode.DeepAssets);
            foreach (Texture2D texture2D in textures)
            {
                TextureImporter ti = (TextureImporter) TextureImporter.GetAtPath(AssetDatabase.GetAssetPath(texture2D));
                TextureImporterPlatformSettings androidTip = ti.GetPlatformTextureSettings("Android");
                TextureImporterPlatformSettings iosTip = new TextureImporterPlatformSettings();
                iosTip.maxTextureSize = androidTip.maxTextureSize;
                iosTip.resizeAlgorithm = androidTip.resizeAlgorithm;
                iosTip.overridden = androidTip.overridden;
                iosTip.compressionQuality = androidTip.compressionQuality;
                iosTip.name = "iPhone";
                iosTip.format = androidTip.format;
                ti.SetPlatformTextureSettings(iosTip);
                ti.SaveAndReimport();
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
#endif
    [MenuItem("Tools/AutoSetImport")]
    public static void SetImport()
    {
        bool autoSetImport = EditorPrefs.GetBool(AutoSetImport);
        EditorPrefs.SetBool(AutoSetImport, !autoSetImport);
        Debug.Log("CurrentAutoSetImport:" + !autoSetImport);
    }
}