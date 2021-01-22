using UnityEngine;
using UnityEngine.UI;

public static class UnityExtension
{
    public static void SetLocalPositionX(this Transform trans, float x)
    {
        Vector3 tempVector3 = trans.localPosition;
        tempVector3.x = x;
        trans.localPosition = tempVector3;
    }

    public static void SetLocalPositionY(this Transform trans, float y)
    {
        Vector3 tempVector3 = trans.localPosition;
        tempVector3.y = y;
        trans.localPosition = tempVector3;
    }

    public static void SetLocalPositionZ(this Transform trans, float z)
    {
        Vector3 tempVector3 = trans.localPosition;
        tempVector3.z = z;
        trans.localPosition = tempVector3;
    }

    public static void SetAnchoredPositionX(this RectTransform rectTrans, float x)
    {
        Vector2 tempVector2 = rectTrans.anchoredPosition;
        tempVector2.x = x;
        rectTrans.anchoredPosition = tempVector2;
    }

    public static void SetAnchoredPositionY(this RectTransform rectTrans, float y)
    {
        Vector2 tempVector2 = rectTrans.anchoredPosition;
        tempVector2.y = y;
        rectTrans.anchoredPosition = tempVector2;
    }

    public static void SetSizeDeltaX(this RectTransform rectTrans, float x)
    {
        Vector2 tempVector2 = rectTrans.sizeDelta;
        tempVector2.x = x;
        rectTrans.sizeDelta = tempVector2;
    }

    public static void SetSizeDeltaY(this RectTransform rectTrans, float y)
    {
        Vector2 tempVector2 = rectTrans.sizeDelta;
        tempVector2.y = y;
        rectTrans.sizeDelta = tempVector2;
    }

    public static RectTransform RectTransform(this Component cp)
    {
        return cp.transform as RectTransform;
    }

    public static RectTransform RectTransform(this GameObject obj)
    {
        return obj.transform as RectTransform;
    }

    public static void SetTransparency(this Graphic graphic, float transparency)
    {
        Color color = graphic.color;
        color.a = transparency;
        graphic.color = color;
    }

    public static void SetTransparency(this Graphic graphic, byte transparency)
    {
        Color32 color = graphic.color;
        color.a = transparency;
        graphic.color = color;
    }

    public static T AddOrGetComponent<T>(this GameObject obj) where T : Component
    {
        T t = obj.GetComponent<T>();
        if (t == null)
        {
            t = obj.AddComponent<T>();
        }

        return t;
    }

    public static T AddOrGetComponent<T>(this Component component) where T : Component
    {
        return AddOrGetComponent<T>(component.gameObject);
    }
}