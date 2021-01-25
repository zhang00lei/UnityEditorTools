using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UGUIEditor
{
    private static bool isMoveUIByArrow = true;
    private const int UILayer = 5;

    [InitializeOnLoadMethod]
    private static void Init()
    {
        SceneView.onSceneGUIDelegate += OnSceneGUI;
    }

    private static void OnSceneGUI(SceneView sceneview)
    {
        Event e = Event.current;
        if (e.type == EventType.KeyDown && isMoveUIByArrow)
        {
            foreach (var item in Selection.transforms)
            {
                Transform trans = item;

                if (trans != null && item.RectTransform() != null)
                {
                    bool isHandled = false;
                    if (e.keyCode == KeyCode.UpArrow)
                    {
                        trans.SetLocalPositionY(trans.localPosition.y + 1);
                        isHandled = true;
                    }

                    if (e.keyCode == KeyCode.DownArrow)
                    {
                        trans.SetLocalPositionY(trans.localPosition.y - 1);
                        isHandled = true;
                    }

                    if (e.keyCode == KeyCode.RightArrow)
                    {
                        trans.SetLocalPositionX(trans.localPosition.x + 1);
                        isHandled = true;
                    }

                    if (e.keyCode == KeyCode.LeftArrow)
                    {
                        trans.SetLocalPositionX(trans.localPosition.x - 1);
                        isHandled = true;
                    }

                    if (isHandled)
                    {
                        Event.current.Use();
                        EditorUtility.SetDirty(trans.gameObject);
                    }
                }
            }
        }
    }

    [MenuItem("Tools/UGUI/Text #&L")]
    public static void CreateText()
    {
        if (Selection.gameObjects.Length > 0)
        {
            GameObject obj = Selection.gameObjects[0];
            GameObject text = new GameObject();
            RectTransform textRect = text.AddComponent<RectTransform>();
            Text textTx = text.AddComponent<Text>();
            text.transform.SetParent(obj.transform);
            text.name = "Text";
            text.layer = UILayer;
            textTx.text = "New Text";
            textTx.color = Color.black;
            textTx.raycastTarget = false;
            textTx.alignment = TextAnchor.MiddleCenter;
            textRect.sizeDelta = new Vector2(160, 30);
            RectTransformZero(textRect);
        }
    }

    [MenuItem("Tools/UGUI/Button #&B")]
    public static void CreateButton()
    {
        if (Selection.gameObjects.Length > 0)
        {
            GameObject obj = Selection.gameObjects[0];

            GameObject button = new GameObject();
            GameObject buttonTx = new GameObject();

            RectTransform buttonRect = button.AddComponent<RectTransform>();
            RectTransform buttonTxRect = buttonTx.AddComponent<RectTransform>();

            button.AddComponent<Image>();
            button.AddComponent<Button>();

            button.transform.SetParent(obj.transform);
            buttonTx.transform.SetParent(button.transform);
            button.name = "Button";

            Text textTemp = buttonTx.AddComponent<Text>();
            buttonTx.name = "Text";
            textTemp.text = "Text";
            textTemp.color = Color.black;
            textTemp.alignment = TextAnchor.MiddleCenter;
            textTemp.raycastTarget = false;
            button.layer = UILayer;
            buttonTx.layer = UILayer;

            RectTransformZero(buttonRect);
            RectTransformZero(buttonTxRect);
        }
    }

    [MenuItem("Tools/UGUI/Image #&S")]
    public static void CreateImage()
    {
        if (Selection.gameObjects.Length > 0)
        {
            GameObject obj = Selection.gameObjects[0];

            GameObject image = new GameObject();
            RectTransform imageRect = image.AddComponent<RectTransform>();
            image.AddComponent<Image>();
            image.transform.SetParent(obj.transform);
            image.name = "Image";
            image.layer = UILayer;
            image.GetComponent<Image>().raycastTarget = false;

            RectTransformZero(imageRect);
        }
    }

    [MenuItem("Tools/UGUI/InputField #&I")]
    public static void CreateInputField()
    {
        if (Selection.gameObjects.Length <= 0)
        {
            return;
        }

        GameObject obj = Selection.gameObjects[0];

        GameObject inputField = new GameObject();
        RectTransform rectTransform = inputField.AddComponent<RectTransform>();
        inputField.AddComponent<Image>();
        inputField.AddComponent<InputField>();
        inputField.layer = UILayer;
        rectTransform.sizeDelta = new Vector2(160, 30);
        inputField.transform.SetParent(obj.transform);
        inputField.name = "InputField";
        RectTransformZero(rectTransform);

        GameObject placeholder = new GameObject();
        Text placeholderTx = placeholder.AddComponent<Text>();
        placeholder.transform.SetParent(inputField.transform);
        rectTransform = placeholderTx.GetComponent<RectTransform>();
        placeholder.name = "Placeholder";
        placeholderTx.text = "Enter text...";
        placeholderTx.alignment = TextAnchor.MiddleLeft;
        placeholderTx.fontStyle = FontStyle.Italic;
        placeholder.layer = UILayer;
        placeholderTx.color = Color.grey;
        rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 0);
        rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, 0);
        rectTransform.anchorMax = Vector2.one;
        rectTransform.anchorMin = Vector2.zero;
        RectTransformZero(rectTransform);

        GameObject text = new GameObject();
        Text textTx = text.AddComponent<Text>();
        text.transform.SetParent(inputField.transform);
        rectTransform = placeholderTx.GetComponent<RectTransform>();
        text.name = "Text";
        text.layer = UILayer;
        textTx.color = Color.black;
        rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 0);
        rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, 0);
        rectTransform.anchorMax = Vector2.one;
        rectTransform.anchorMin = Vector2.zero;
        RectTransformZero(rectTransform);
    }

    [MenuItem("Tools/UGUI/EmptyObj #&G")]
    public static void CreateEmptyChildObj()
    {
        GameObject obj = Selection.gameObjects[0];
        GameObject emptyObj = new GameObject();
        emptyObj.AddComponent<RectTransform>();
        emptyObj.name = "GameObject";
        emptyObj.transform.SetParent(obj.transform);
        RectTransformZero(emptyObj.transform.RectTransform());
        emptyObj.layer = UILayer;
    }

    private static void RectTransformZero(RectTransform rectTransform)
    {
        rectTransform.localScale = Vector3.one;
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.anchoredPosition3D = Vector3.zero;
    }
}