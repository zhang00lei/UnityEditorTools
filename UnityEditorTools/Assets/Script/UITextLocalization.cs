using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class TextData
{
    public Text text;

    public int langId;

    public TextData(Text text, int langId)
    {
        this.text = text;
        this.langId = langId;
    }
}

public class UITextLocalization : MonoBehaviour
{
    [HideInInspector] public List<TextData> datas = new List<TextData>();

    private void Awake()
    {
//        对应自己的多语言管理器
//        var langMgr = LanguageManager.Instance;
        for (int i = 0; i < datas.Count; i++)
        {
            int id = datas[i].langId;
//            获取对应的文字信息
//            string str = langMgr.GetLang(id);
            string str = "";
            if (str == null)
            {
                Debug.LogError($"UI:{gameObject.name}--没有找到id = {id}的多语言字段！检查配表");
            }
            else
            {
                datas[i].text.text = str;
            }
        }
    }

    private void OnEnable()
    {
        //添加切换语言事件
    }

    private void OnDisable()
    {
        //移除切换语言事件
    }
}