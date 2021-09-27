using System;
using System.Collections.Generic;
using UnityEngine;

public enum ReferenceType
{
    Transform,
    GameObject,
    UIText,
    UITextMeshProUGUI,
    UIButton,
    UIImage,
    UISlider,
    UIInput,
    Animator,
    UICanvas,
}

[Serializable]
public class ReferenceCollectorData
{
    public string key;

    public ReferenceType type;

    public GameObject gameObject;
}


public class UIElementsGenerate : MonoBehaviour
{
    public List<ReferenceCollectorData> data = new List<ReferenceCollectorData>();
    public string SavePath;
}