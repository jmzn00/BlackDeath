using System;
using System.Collections.Generic;

public static class EventBus
{
    private static Dictionary<Type, List<Delegate>> _subscribers = new();
    private static Queue<object> _eventQueue = new();

    public static void Subscribe<T>(Action<T> callback) where T : struct
    {
        var type = typeof(T);
        if (!_subscribers.ContainsKey(type))
            _subscribers[type] = new List<Delegate>();
        _subscribers[type].Add(callback);
    }
    public static void Unsubscribe<T>(Action<T> callback) where T : struct
    {
        var type = typeof(T);
        if (_subscribers.ContainsKey(type))
            _subscribers[type].Remove(callback);
    }
    public static void Publish<T>(T evt) where T : struct
    {
        var type = typeof(T);
        if (!_subscribers.ContainsKey(type)) return;

        foreach (var callback in _subscribers[type])
        {
            ((Action<T>)callback)?.Invoke(evt);
        }
    }
    public static void QueueEvent<T>(T evt) where T : struct
    {
        _eventQueue.Enqueue(evt);
    }
    public static void ProcessQueue()
    {
        int count = _eventQueue.Count;
        for (int i = 0; i < count; i++)
        {
            object evt = _eventQueue.Dequeue();
            var type = evt.GetType();
            if (_subscribers.ContainsKey(type))
            {
                foreach (var callback in _subscribers[type])
                {
                    callback.DynamicInvoke(evt);
                }
            }
        }
    }
}