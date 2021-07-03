using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;


[CustomEditor(typeof(UITextLocalization))]
public class UITextLocalizationEditor : Editor
{
    private UITextLocalization _target;
    private Rect textObjRect;
    private Vector3 scrollPos = Vector2.zero;

    private GUIContent guiContentTitle =
        new GUIContent("Drag Text here from Hierarchy view to get the object");


    private void OnEnable()
    {
        _target = target as UITextLocalization;
    }


    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        textObjRect = GUILayoutUtility.GetRect(0f, 30, GUILayout.ExpandWidth(true));
        GUIStyle guiStyle = new GUIStyle();
        guiStyle.alignment = TextAnchor.MiddleCenter;
        guiStyle.normal.background = new Texture2D(1, 1);
        guiStyle.normal.background.SetPixel(0, 0, Color.red);
        GUI.color = Color.green;
        GUI.Box(textObjRect, guiContentTitle, guiStyle);
        GUI.color = Color.white;
        if ((Event.current.type == EventType.DragUpdated || Event.current.type == EventType.DragExited) &&
            textObjRect.Contains(Event.current.mousePosition))
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
            if (DragAndDrop.objectReferences != null && DragAndDrop.objectReferences.Length > 0)
            {
                Text text = (DragAndDrop.objectReferences[0] as GameObject).GetComponent<Text>();
                if (text != null)
                {
                    if (_target.datas.Find(x => x.text == text) == null)
                    {
                        _target.datas.Add(new TextData(text, 0));
                        EditorUtility.SetDirty(_target);
                    }
                }
                else
                {
                    Debug.LogError("Wrong type of drag and drop object");
                }
            }
        }

        AutoRefresh();

        EditorGUILayout.EndScrollView();
    }


    private void AutoRefresh()
    {
        for (int i = 0; i < _target.datas.Count; i++)
        {
            TextData data = _target.datas[i];

            GUILayout.BeginHorizontal();
            EditorGUILayout.ObjectField(data.text, typeof(TextMeshProUGUI), false, GUILayout.Width(150));
            data.text.text = EditorGUILayout.TextField(data.text.text, GUILayout.Width(150));
            var str = EditorGUILayout.TextField(data.langId.ToString(), GUILayout.Width(70));
            data.langId = int.Parse(str);
            if (GUILayout.Button("X"))
            {
                _target.datas.RemoveAt(i);
                i--;
                EditorUtility.SetDirty(_target);
            }

            GUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Refresh"))
        {
            Text[] tmps = _target.gameObject.GetComponentsInChildren<Text>(true);
            for (int i = 0; i < tmps.Length; i++)
            {
                TextData data = _target.datas.Find(x => x.text == tmps[i]);
                if (data == null)
                {
                    _target.datas.Add(new TextData(tmps[i], 0));
                }
            }

            for (int i = 0; i < _target.datas.Count; i++)
            {
                if (_target.datas[i].text == null)
                {
                    _target.datas.RemoveAt(i);
                    i--;
                }
            }

            EditorUtility.SetDirty(_target);
        }
    }
}