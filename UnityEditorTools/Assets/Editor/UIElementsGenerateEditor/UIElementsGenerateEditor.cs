using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using TMPro;

[CustomEditor(typeof(UIElementsGenerate))]
[CanEditMultipleObjects]
public class UIElementsGenerateEditor : Editor
{
    private UIElementsGenerate referenceCollector;
    private SerializedProperty dataProperty;

    private class ComponentInfo
    {
        public string annotionName;
        public Type componentType;

        public ComponentInfo(string annotionName, Type componentType)
        {
            this.annotionName = annotionName;
            this.componentType = componentType;
        }
    }

    [SerializeField] private Dictionary<ReferenceType, ComponentInfo> componentAnnotationDict =
        new Dictionary<ReferenceType, ComponentInfo>
        {
            {ReferenceType.GameObject, new ComponentInfo("UnityEngine.GameObject", typeof(GameObject))},
            {ReferenceType.UIText, new ComponentInfo("UIText", typeof(Text))},
            {ReferenceType.UITextMeshProUGUI, new ComponentInfo("UITextMeshProUGUI", typeof(TextMeshProUGUI))},
            {ReferenceType.UIButton, new ComponentInfo("UIButton", typeof(Button))},
            {ReferenceType.UIImage, new ComponentInfo("UIImage", typeof(Image))},
            {ReferenceType.UISlider, new ComponentInfo("UISlider", typeof(Slider))},
            {ReferenceType.UIInput, new ComponentInfo("UIInput", typeof(InputField))},
            {ReferenceType.Animator, new ComponentInfo("UnityEngine.Animator", typeof(Animator))},
            {ReferenceType.Transform, new ComponentInfo("UnityEngine.Transform", typeof(Transform))},
            {ReferenceType.UICanvas, new ComponentInfo("UICanvas", typeof(Canvas))},
        };

    private void OnEnable()
    {
        referenceCollector = (UIElementsGenerate) target;
        dataProperty = serializedObject.FindProperty("data");
        if (string.IsNullOrEmpty(referenceCollector.SavePath))
        {
            string uiName = referenceCollector.gameObject.name;
            string filePath = $"UI/{uiName}/View/{uiName}Elements.lua";
            referenceCollector.SavePath = filePath;
        }
    }

    public override void OnInspectorGUI()
    {
        GUILayout.BeginHorizontal();
        var savePath = serializedObject.FindProperty("SavePath");
        savePath.stringValue = GUILayout.TextField(savePath.stringValue);
        if (GUILayout.Button("Export", GUILayout.Width(100)))
        {
            ExportLuaTable();
        }

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("ClearAll"))
        {
            referenceCollector.data.Clear();
        }

        if (GUILayout.Button("ClearEmptyReference"))
        {
            DelNullReference();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        var delList = new List<int>();
        SerializedProperty property;
        for (int i = 0; i < referenceCollector.data.Count; i++)
        {
            GUILayout.BeginHorizontal();

            property = dataProperty.GetArrayElementAtIndex(i).FindPropertyRelative("key");
            property.stringValue = EditorGUILayout.TextField(property.stringValue, GUILayout.Width(120));

            property = dataProperty.GetArrayElementAtIndex(i).FindPropertyRelative("type");
            EditorGUILayout.PropertyField(property, GUIContent.none);

            property = dataProperty.GetArrayElementAtIndex(i).FindPropertyRelative("gameObject");
            EditorGUILayout.ObjectField(property.objectReferenceValue, typeof(Object), true);
            if (GUILayout.Button("X"))
            {
                delList.Add(i);
            }

            GUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();

        DragToHere(dataProperty);

        foreach (var i in delList)
        {
            dataProperty.DeleteArrayElementAtIndex(i);
        }

        serializedObject.ApplyModifiedProperties();
        serializedObject.UpdateIfRequiredOrScript();
    }


    private void DragToHere(SerializedProperty dataProperty)
    {
        var eventType = Event.current.type;
        if (eventType == EventType.DragUpdated || eventType == EventType.DragPerform)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            if (eventType == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                foreach (var o in DragAndDrop.objectReferences)
                {
                    AddReference(dataProperty, o.name, o);
                }
            }

            Event.current.Use();
        }
    }

    private void AddReference(SerializedProperty dataProperty, string key, Object obj)
    {
        int index = dataProperty.arraySize;
        dataProperty.InsertArrayElementAtIndex(index);

        var element = dataProperty.GetArrayElementAtIndex(index);
        element.FindPropertyRelative("key").stringValue = key;
        element.FindPropertyRelative("type").enumValueIndex = (int) GetTypeIndex(obj);
        element.FindPropertyRelative("gameObject").objectReferenceValue = obj;
    }

    private void DelNullReference()
    {
        for (int i = referenceCollector.data.Count - 1; i >= 0; i--)
        {
            var refrence = referenceCollector.data[i];
            if (refrence.gameObject == null)
            {
                referenceCollector.data.RemoveAt(i);
            }
        }
    }

    private ReferenceType GetTypeIndex(Object obj)
    {
        var go = obj as GameObject;
        if (go == null)
        {
            return 0;
        }

        foreach (var componentInfo in componentAnnotationDict)
        {
            if (componentInfo.Key == ReferenceType.GameObject)
            {
                continue;
            }

            if (go.GetComponent(componentInfo.Value.componentType) != null)
            {
                return componentInfo.Key;
            }
        }

        return ReferenceType.Transform;
    }

    private void ExportLuaTable()
    {
        if (string.IsNullOrEmpty(referenceCollector.SavePath))
        {
            EditorUtility.DisplayDialog("Error", "The path cannot be empty", "Confirm");
            return;
        }

        string fileName = Path.GetFileName(referenceCollector.SavePath);
        if (string.IsNullOrEmpty(fileName))
        {
            EditorUtility.DisplayDialog("Error", "The file name is error", "Confirm");
            return;
        }

        if (!fileName.EndsWith("Elements.lua"))
        {
            EditorUtility.DisplayDialog("Error", "The file name should end with \"Elements.lua\"", "Confirm");
            return;
        }

        foreach (var reference in referenceCollector.data)
        {
            if (componentAnnotationDict[reference.type].annotionName.StartsWith("UnityEngine."))
            {
                continue;
            }

            var component = reference.gameObject.GetComponent(componentAnnotationDict[reference.type].componentType);
            if (component == null)
            {
                EditorUtility.DisplayDialog("Error",
                    $"The 【{reference.key}】can not contain 【{reference.type}】 component",
                    "Confirm");
                return;
            }
        }

        SerializedProperty dataProperty = serializedObject.FindProperty("data");
        int length = dataProperty.arraySize;

        List<string> checkKeyInfo = new List<string>();
        for (int i = 0;
            i < length;
            i++)
        {
            var element = dataProperty.GetArrayElementAtIndex(i);
            string keyInfo = element.FindPropertyRelative("key").stringValue;
            if (checkKeyInfo.Contains(keyInfo))
            {
                EditorUtility.DisplayDialog("Error", $"The key 【{keyInfo}】 is duplication", "Confirm");
                return;
            }

            checkKeyInfo.Add(keyInfo);
        }

        string className = fileName.Replace(".lua", string.Empty);
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("---this file is generate by tools,do not modify it.");
        sb.AppendLine($"---@class {className}");
        sb.AppendLine($"local {className} = {{}}");
        sb.AppendLine("---Init 初始化UI元素");
        string temp = fileName.Replace("Elements.lua", string.Empty);
        sb.AppendLine($"---@param tableInfo {temp}");
        sb.AppendLine($"function {className}:Init(tableInfo)");
        foreach (var refrence in referenceCollector.data)
        {
            string annotationStr = GetElementAnnotation(refrence.type);
            string objPath = GetObjPath(refrence.gameObject, referenceCollector.gameObject);
            if (!string.IsNullOrEmpty(annotationStr))
            {
                sb.AppendLine($"    ---@type {annotationStr}");
                if (refrence.type == ReferenceType.Transform)
                {
                    sb.AppendLine(
                        $"    tableInfo.{refrence.key} = UIUtil.FindTrans(tableInfo.transform, \"{objPath}\")");
                }
                else if (refrence.type == ReferenceType.GameObject)
                {
                    sb.AppendLine(
                        $"    tableInfo.{refrence.key} = UIUtil.FindTrans(tableInfo.transform, \"{objPath}\").gameObject");
                }
                else if (refrence.type == ReferenceType.Animator)
                {
                    sb.AppendLine(
                        $"    tableInfo.{refrence.key} = UIUtil.FindAnimator(tableInfo.transform, \"{objPath}\")");
                }
                else
                {
                    sb.AppendLine(
                        $"    tableInfo.{refrence.key} = tableInfo:AddComponent({annotationStr}, \"{objPath}\")");
                }
            }
        }

        sb.AppendLine("end");
        sb.AppendLine();

        sb.AppendLine($"function {className}:Destroy(tableInfo)");
        foreach (var refrence in referenceCollector.data)
        {
            sb.AppendLine($"    tableInfo.{refrence.key} = nil");
        }

        sb.AppendLine("end");
        sb.AppendLine();

        sb.AppendLine($"return {className}");

        string scriptPath = Path.Combine(Application.dataPath, "LuaScripts", referenceCollector.SavePath);
        scriptPath = scriptPath.Replace("/", "\\");
        string directoryPath = scriptPath.Substring(0, scriptPath.LastIndexOf('\\'));

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        File.WriteAllText(scriptPath, sb.ToString());

        AssetDatabase.Refresh();
        Debug.Log("文件保存到成功:\n" + scriptPath);
    }


    private string GetElementAnnotation(ReferenceType referenceType)
    {
        if (componentAnnotationDict.ContainsKey(referenceType))
        {
            return componentAnnotationDict[referenceType].annotionName;
        }

        return string.Empty;
    }

    private string GetObjPath(GameObject checkObj, GameObject endObj)
    {
        string result = string.Empty;

        Transform selectionTrans = checkObj.transform;
        while (selectionTrans != null)
        {
            result = $"{selectionTrans.name}/{result}";
            selectionTrans = selectionTrans.parent;
            if (selectionTrans.gameObject == endObj)
            {
                result = result.Substring(0, result.Length - 1);
                break;
            }
        }

        return result;
    }
}