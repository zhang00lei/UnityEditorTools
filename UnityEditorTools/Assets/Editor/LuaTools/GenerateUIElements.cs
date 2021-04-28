using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEngine;

public class GenerateUIElements : EditorWindow
{
    private const string SPACING = "    ";
    private static string savePathInfo;
    private static Action genUnityComponentCode;
    private static Action genLuaComponentCode;

    private class ComponentTypeInfo
    {
        /// <summary>
        /// 注解
        /// </summary>
        public string annotationName;

        /// <summary>
        /// 字段类型
        /// </summary>
        public string fieldType;

        /// <summary>
        /// Lua组件名字
        /// </summary>
        public string luaComponentName;

        public ComponentTypeInfo(string annotationName, string fieldType, string luaComponentName)
        {
            this.annotationName = annotationName;
            this.fieldType = fieldType;
            this.luaComponentName = luaComponentName;
        }
    }

    private static Dictionary<string, ComponentTypeInfo> typeInfoStructDict =
        new Dictionary<string, ComponentTypeInfo>()
        {
            {"Img", new ComponentTypeInfo("UnityEngine.UI.Image", "CS.UnityEngine.UI.Image", "UIImage")},
            {"Text", new ComponentTypeInfo("TMPro.TextMeshProUGUI", "CS.TMPro.TextMeshProUGUI", "UITextMeshProUGUI")},
            {"Txt", new ComponentTypeInfo("UnityEngine.UI.Text", "CS.UnityEngine.UI.Image", "UIText")},
            {"Btn", new ComponentTypeInfo("UnityEngine.UI.Button", "CS.UnityEngine.UI.Button", "UIButton")},
            {"Trans", new ComponentTypeInfo("UnityEngine.Transform", "CS.UnityEngine.Transform", "")},
            {"Obj", new ComponentTypeInfo("UnityEngine.GameObject", "CS.UnityEngine.Transform", "")},
            {"Input", new ComponentTypeInfo("UnityEngine.UI.InputField", "CS.UnityEngine.UI.InputField", "UIInput")},
            {"Slider", new ComponentTypeInfo("UnityEngine.UI.Slider", "CS.nityEngine.UI.Slider", "UISlider")},
            {
                "Ske",
                new ComponentTypeInfo("Spine.Unity.SkeletonGraphic", "CS.Spine.Unity.SkeletonGraphic",
                    "UISkeletonGraphic")
            },
            {"Tog", new ComponentTypeInfo("UnityEngine.UI.Toggle", "CS.UnityEngine.UI.Toggle", "")}
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
            savePathInfo = $"LuaScripts/UI/{transform.name}/View/{transform.name}ViewElements.lua";
            assetImporter.userData = savePathInfo;
            assetImporter.SaveAndReimport();
        }

        List<string> transPathList = new List<string>();
        GetAllChild(transform, string.Empty, transPathList);

        Open();
        List<string> componentList = FilterComponent(transPathList);
        genUnityComponentCode = () => { GenCode(transform.name, componentList, assetImporter, false); };
        genLuaComponentCode = () => { GenCode(transform.name, componentList, assetImporter, true); };
    }

    private static void GenCode(string fileName, List<string> transPathList, AssetImporter assetImporter,
        bool isGenLuaComponent)
    {
        string luaCode = isGenLuaComponent
            ? GenLuaComponentCode(fileName, transPathList)
            : GenUnityComponentCode(fileName, transPathList);
        assetImporter.userData = savePathInfo;
        assetImporter.SaveAndReimport();
        if (!string.IsNullOrEmpty(luaCode))
        {
            string savePath = Path.Combine(Application.dataPath, savePathInfo);
            File.WriteAllText(savePath, luaCode);
            AssetDatabase.Refresh();
        }
    }

    private static List<string> FilterComponent(List<string> transPathList)
    {
        List<string> fieldInfoList = new List<string>();
        foreach (var pathStr in transPathList)
        {
            var typeInfo = GetComponentTypeInfo(pathStr);
            if (typeInfo != null)
            {
                fieldInfoList.Add(pathStr);
            }
        }

        return fieldInfoList;
    }

    private static string GenUnityComponentCode(string fileName, List<string> fieldInfoList)
    {
        fileName += "ViewElements";
        if (fieldInfoList.Count != 0)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("---this file is generate by tools,do not modify it.");
            sb.AppendLine($"---@class {fileName}");
            sb.AppendLine($"local {fileName} = {{}}");
            sb.AppendLine("---Init 初始化UI元素");
            sb.AppendLine("---@param transform UnityEngine.Transform");
            sb.AppendLine($"function {fileName}:Init(transform)");
            sb.AppendLine($"{SPACING}self.transform = transform");
            sb.AppendLine($"{SPACING}self.gameObject = transform.gameObject");
            foreach (string fieldInfo in fieldInfoList)
            {
                string fieldName = GetFieldName(fieldInfo);
                ComponentTypeInfo componentTypeInfo = GetComponentTypeInfo(fieldInfo);
                if (componentTypeInfo == null)
                {
                    continue;
                }

                string fieldType = componentTypeInfo.fieldType;
                string annotionName = componentTypeInfo.annotationName;
                sb.AppendLine($"{SPACING}---@type {annotionName}");
                sb.Append(
                    $"{SPACING}self.{fieldName} = UIUtil.FindComponent(transform, typeof({fieldType}), \"{fieldInfo}\")");
                if (annotionName.Contains("GameObject"))
                {
                    sb.Append(".gameObject");
                }

                sb.AppendLine();
            }

            sb.AppendLine("end");
            sb.AppendLine();
            sb.AppendLine($"function {fileName}:OnDestroy()");
            sb.AppendLine($"{SPACING}self.transform = nil");
            sb.AppendLine($"{SPACING}self.gameObject = nil");
            foreach (string fieldInfo in fieldInfoList)
            {
                string fieldName = GetFieldName(fieldInfo);
                var componentTypeInfo = GetComponentTypeInfo(fieldInfo);
                if (componentTypeInfo == null)
                {
                    continue;
                }

                string fieldType = componentTypeInfo.annotationName;

                if (fieldType.EndsWith("Button"))
                {
                    sb.AppendLine($"{SPACING}self.{fieldName}.onClick:RemoveAllListeners()");
                }

                sb.AppendLine($"{SPACING}self.{fieldName} = nil");
            }

            sb.AppendLine("end");

            sb.AppendLine($"\nreturn {fileName}");
            return sb.ToString();
        }

        return string.Empty;
    }

    private static string GenLuaComponentCode(string fileName, List<string> fieldInfoList)
    {
        string saveFileName = fileName + "ViewElements";
        if (fieldInfoList.Count == 0)
        {
            return string.Empty;
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("---this file is generate by tools,do not modify it.");
        sb.AppendLine($"---@class {saveFileName}");
        sb.AppendLine($"local {saveFileName} = {{}}");
        sb.AppendLine("---Init 初始化UI元素");
        sb.AppendLine($"---@param tableInfo {fileName}View");
        sb.AppendLine($"function {saveFileName}:Init(tableInfo)");
        foreach (string fieldInfo in fieldInfoList)
        {
            string fieldName = GetFieldName(fieldInfo);
            ComponentTypeInfo componentTypeInfo = GetComponentTypeInfo(fieldInfo);
            if (componentTypeInfo == null)
            {
                continue;
            }

            if (!string.IsNullOrEmpty(componentTypeInfo.luaComponentName))
            {
                sb.AppendLine($"{SPACING}---@type {componentTypeInfo.luaComponentName}");
                sb.AppendLine(
                    $"{SPACING}tableInfo.{fieldName} = tableInfo:AddComponent({componentTypeInfo.luaComponentName}, \"{fieldInfo}\")");
            }
            else
            {
                sb.AppendLine($"{SPACING}---@type {componentTypeInfo.annotationName}");
                sb.Append(
                    $"{SPACING}tableInfo.{fieldName} = UIUtil.FindComponent(tableInfo.transform ,typeof({componentTypeInfo.fieldType}), \"{fieldInfo}\")");
                if (componentTypeInfo.annotationName.Contains("GameObject"))
                {
                    sb.Append(".gameObject");
                }

                sb.AppendLine();
            }
        }

        sb.AppendLine("end");
        sb.AppendLine();
        sb.AppendLine($"function {saveFileName}:Destroy(tableInfo)");

        foreach (string fieldInfo in fieldInfoList)
        {
            string fieldName = GetFieldName(fieldInfo);
            var componentTypeInfo = GetComponentTypeInfo(fieldInfo);
            if (componentTypeInfo == null)
            {
                continue;
            }

            sb.AppendLine($"{SPACING}tableInfo.{fieldName} = nil");
        }
        sb.AppendLine("end");

        sb.AppendLine($"\nreturn {saveFileName}");
        return sb.ToString();
    }

    private static string GetFieldName(string fieldPath)
    {
        string[] arrTemp = fieldPath.Split('/');
        return arrTemp[arrTemp.Length - 1];
    }

    private static ComponentTypeInfo GetComponentTypeInfo(string path)
    {
        foreach (var keyValuePair in typeInfoStructDict)
        {
            string[] objectNames = path.Split('/');
            string objectName = objectNames[objectNames.Length - 1];
            if (path.EndsWith(keyValuePair.Key) && !objectName.Equals(keyValuePair.Key))
            {
                return keyValuePair.Value;
            }
        }

        return null;
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
            500, 50);
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
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("GenUnityComponent"))
        {
            genUnityComponentCode?.Invoke();
        }

        if (GUILayout.Button("GenLuaComponent"))
        {
            genLuaComponentCode?.Invoke();
        }

        GUILayout.EndHorizontal();
    }

    private void OnDisable()
    {
        savePathInfo = string.Empty;
        genUnityComponentCode = null;
    }
}