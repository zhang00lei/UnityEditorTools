using System;
using GameFramework;
using GameFramework.Event;

public class TestEventArgs : GameEventArgs
{
    public string testInfo;

    public static TestEventArgs Create(string str)
    {
        TestEventArgs temp = ReferencePool.Acquire<TestEventArgs>();
        temp.testInfo = str;
        return temp;
    }

    public override void Clear()
    {
        testInfo = default;
    }

    public override int Id => EventId;

    public static readonly int EventId = typeof(TestEventArgs).GetHashCode();
}