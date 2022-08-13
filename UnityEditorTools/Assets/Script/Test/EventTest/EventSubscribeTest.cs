using System;
using GameFramework.Event;
using UnityEngine;
using UnityEngine.UI;

public class EventSubscribeTest : MonoBehaviour
{
    private Text testText;

    private void Start()
    {
        testText = transform.Find("testText").GetComponent<Text>();
        
        EventManager.Instance.Subscribe(TestEventArgs.EventId, OnTestEventFire);
    }

    private void OnEnable()
    {
    }

    private void OnDisable()
    {
        EventManager.Instance.Unsubscribe(TestEventArgs.EventId, OnTestEventFire);
    }

    private void OnTestEventFire(object sender, GameEventArgs e)
    {
        TestEventArgs temp = e as TestEventArgs;
        testText.text = temp.testInfo;
    }
}