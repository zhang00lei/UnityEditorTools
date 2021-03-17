using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEngine;

public class GenerateUIElements : EditorWindow
{
    private static string savePathInfo;
    private static Action generateLuaCode;

    private struct TypeInfoStruct
    {
        public readonly string annotationName;
        public readonly string fieldType;

        public TypeInfoStruct(string annotationName, string fieldType)
        {
            this.annotationName = annotationName;
            this.fieldType = fieldType;
        }
    }

    private static readonly Dictionary<string, TypeInfoStruct> typeInfoStructDict = new Dictionary<string, TypeInfoStruct>
    {
        {"Img", new TypeInfoStruct("UnityEngine.UI.Image", "CS.UnityEngine.UI.Image")},
        {"Text", new TypeInfoStruct("TMPro.TextMeshProUGUI", "CS.TMPro.TextMeshProUGUI")},
        {"Btn", new TypeInfoStruct("UnityEngine.UI.Button", "CS.UnityEngine.UI.Button")},
        {"Trans", new TypeInfoStruct("UnityEngine.Transform", "CS.UnityEngine.Transform")},
        {"Obj", new TypeInfoStruct("UnityEngine.GameObject", "CS.UnityEngine.Transform")}
    };

    [MenuItem("GameObject/GenUILuaCode", priority = 30)]
    public static void GetSelect()
    {
        Transform[] transforms = Selection.transforms;
        if (transforms.Length != 1)
        {
            return;
        }

        Transform transform = transforms[0];

        PrefabStage prefab = PrefabStageUtility.GetPrefabStage(transform.gameObject);
        if (prefab == null)
        {
            return;
        }

        AssetImporter assetImporter = AssetImporter.GetAtPath(prefab.prefabAssetPath);
        savePathInfo = assetImporter.userData;
        if (string.IsNullOrEmpty(savePathInfo))
        {
            //Lua文件保存路径
            savePathInfo = $"LuaScripts/UI/{transform.name}/View/{transform.name}ViewElements.lua";
            assetImporter.userData = savePathInfo;
            assetImporter.SaveAndReimport();
        }

        List<string> transPathList = new List<string>();
        GetAllChild(transform, string.Empty, transPathList);
        string fileName = transform.name + "ViewElements";

        Open();
        generateLuaCode = () =>
        {
            string luaCode = GenLuaCode(fileName, transPathList);
            assetImporter.userData = savePathInfo;
            assetImporter.SaveAndReimport();
            if (!string.IsNullOrEmpty(luaCode))
            {
                string savePath = Path.Combine(Application.dataPath, savePathInfo);
                File.WriteAllText(savePath, luaCode);
                AssetDatabase.Refresh();
            }
        };
    }

    private static string GenLuaCode(string fileName, List<string> transPathList)
    {
        string spacing = "    ";
        List<string> fieldInfoList = new List<string>();
        foreach (var pathStr in transPathList)
        {
            string typeTemp = GetFieldType(pathStr);
            if (!string.IsNullOrEmpty(typeTemp))
            {
                fieldInfoList.Add(pathStr);
            }
        }

        if (fieldInfoList.Count != 0)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("---this file is generate by tools,do not modify it.");
            sb.AppendLine($"---@class {fileName}");
            sb.AppendLine("---@field public transform UnityEngine.Transform");
            sb.AppendLine("---@field public gameObject UnityEngine.GameObject");
            foreach (string fieldInfo in fieldInfoList)
            {
                string fieldName = GetFieldName(fieldInfo);
                string fieldType = GetFieldType(fieldInfo);
                sb.AppendLine($"---@field public {fieldName} {fieldType}");
            }

            sb.AppendLine("---Init 初始化UI元素");
            sb.AppendLine("---@param transform UnityEngine.Transform");
            sb.AppendLine($"function {fileName}:Init(transform)");
            sb.AppendLine($"{spacing}self.transform = transform");
            sb.AppendLine($"{spacing}self.gameObject = transform.gameObject");
            foreach (string fieldInfo in fieldInfoList)
            {
                string fieldName = GetFieldName(fieldInfo);
                string fieldType = GetFieldType(fieldInfo, false);
                string annotionName = GetFieldType(fieldInfo);
                sb.Append(
                    $"{spacing}self.{fieldName} = UIUtil.FindComponent(transform, typeof({fieldType}) \"{fieldInfo}\")");
                if (annotionName.Contains("GameObject"))
                {
                    sb.Append(".gameObject");
                }

                sb.AppendLine();
            }

            sb.AppendLine("end");
            sb.AppendLine();
            sb.AppendLine($"function {fileName}:OnDestroy()");
            sb.AppendLine($"{spacing}self.transform = nil");
            sb.AppendLine($"{spacing}self.gameObject = nil");
            foreach (string fieldInfo in fieldInfoList)
            {
                string fieldName = GetFieldName(fieldInfo);
                string fieldType = GetFieldType(fieldInfo, false);
                if (fieldType.EndsWith("Button"))
                {
                    sb.AppendLine($"{spacing}self.{fieldName}.onClick:RemoveAllListeners()");
                }

                sb.AppendLine($"{spacing}self.{fieldName} = nil");
            }

            sb.AppendLine("end");

            sb.AppendLine($"\nreturn {fileName}");
            return sb.ToString();
        }

        return string.Empty;
    }

    private static string GetFieldName(string fieldPath)
    {
        string[] arrTemp = fieldPath.Split('/');
        return arrTemp[arrTemp.Length - 1];
    }

    private static string GetFieldType(string path, bool isAnnotiationName = true)
    {
        foreach (var keyValuePair in typeInfoStructDict)
        {
            if (path.EndsWith(keyValuePair.Key))
            {
                if (isAnnotiationName)
                {
                    return keyValuePair.Value.annotationName;
                }

                return keyValuePair.Value.fieldType;
            }
        }

        return string.Empty;
    }

    private static void GetAllChild(Transform trans, string path, List<string> transPathList)
    {
        for (int i = 0; i < trans.childCount; i++)
        {
            Transform transTemp = trans.GetChild(i);
            string pathTemp = path + transTemp.name;
            transPathList.Add(pathTemp);
            if (transTemp.childCount != 0)
            {
                pathTemp += "/";
                GetAllChild(transTemp, pathTemp, transPathList);
            }
        }
    }

    private static void Open()
    {
        GenerateUIElements window = GetWindow<GenerateUIElements>();
        window.position = new Rect(Screen.currentResolution.width / 2 - 250, Screen.currentResolution.height / 2 - 30,
            500, 60);
        window.maxSize = new Vector2(500, 60);
        window.minSize = new Vector2(500, 60);
        window.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("SavePath:", GUILayout.MaxWidth(65));
        savePathInfo = EditorGUILayout.TextField(savePathInfo);
        EditorGUILayout.EndHorizontal();
        if (GUILayout.Button("Confirm"))
        {
            generateLuaCode?.Invoke();
        }
    }

    private void OnDisable()
    {
        savePathInfo = string.Empty;
        generateLuaCode = null;
    }
}