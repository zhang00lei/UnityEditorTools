using System;
using GameFramework.Event;
using UnityEngine;
using UnityEngine.UI;

public class EventFireTest : MonoBehaviour
{
    private void Start()
    {
        transform.Find("testBtn").GetComponent<Button>().onClick.AddListener(OnTestBtnClick);
    }

    private void OnTestBtnClick()
    {
        var temp = TestEventArgs.Create(DateTime.Now.ToString());
        EventManager.Instance.Fire(this, temp);
    }
}