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
        
        if (assetPath.IndexOf("Bundle", StringComparison.Ordinal) < 0)
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
        string assetDirPath = Path.GetDirectoryName(assetPath).Replace("\\", "/");
        assetDirPath = assetDirPath.Replace("Assets/Bundle/", string.Empty);
        AtlasInfo atlasInfo = AtlasSettingTools.GetAtlasInfo(assetDirPath);
        TextureImporterPlatformSettings textureImporterPlatformSettings;
        if (atlasInfo != null)
        {
            textureImporter.spritePackingTag = atlasInfo.atlasName;
            textureImporterPlatformSettings =
                GetTextureImporterPlatformSettings("Android", atlasInfo.androidCompressType, maxTextureSize);
            textureImporter.SetPlatformTextureSettings(textureImporterPlatformSettings);
            textureImporterPlatformSettings =
                GetTextureImporterPlatformSettings("iPhone", atlasInfo.iosCompressType, maxTextureSize);
            textureImporter.SetPlatformTextureSettings(textureImporterPlatformSettings);
            textureImporter.SaveAndReimport();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        else
        {
            textureImporterPlatformSettings =
                GetTextureImporterPlatformSettings("Android", TextureImporterFormat.ASTC_6x6, maxTextureSize);
            textureImporter.SetPlatformTextureSettings(textureImporterPlatformSettings);
            
            textureImporterPlatformSettings =
                GetTextureImporterPlatformSettings("iPhone", TextureImporterFormat.ASTC_6x6, maxTextureSize);
            textureImporter.SetPlatformTextureSettings(textureImporterPlatformSettings);
        }
    }

    private TextureImporterPlatformSettings GetTextureImporterPlatformSettings(string platformName,
        TextureImporterFormat formatType, int size)
    {
        TextureImporterPlatformSettings textureImporterPlatformSettings = new TextureImporterPlatformSettings();
        textureImporterPlatformSettings.androidETC2FallbackOverride = AndroidETC2FallbackOverride.UseBuildSettings;
        textureImporterPlatformSettings.maxTextureSize = GetMaxSize(size);
        textureImporterPlatformSettings.resizeAlgorithm = TextureResizeAlgorithm.Mitchell;
        textureImporterPlatformSettings.overridden = true;
        textureImporterPlatformSettings.compressionQuality = 50;
        textureImporterPlatformSettings.name = platformName;
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

    [MenuItem("Tools/AtlasSetting/AutoSetImport")]
    public static void SetImport()
    {
        bool autoSetImport = EditorPrefs.GetBool(AutoSetImport, false);
        EditorPrefs.SetBool(AutoSetImport, !autoSetImport);
        Debug.Log("CurrentAutoSetImport:" + !autoSetImport);
    }
}