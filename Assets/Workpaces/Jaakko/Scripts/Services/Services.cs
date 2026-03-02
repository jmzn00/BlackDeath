using System;
using System.Collections.Generic;
using UnityEngine;

public static class Services
{
    private static readonly Dictionary<Type, object> m_services = new();

    public static void Register<T>(T service) where T : class 
    {
        var type = typeof(T);

        if (m_services.ContainsKey(type))
            Debug.LogWarning($"Service {type.Name} already registered");
        m_services[type] = service;
    }
    public static T Get<T>() where T : class 
    {
        var type = typeof(T);

        if (m_services.TryGetValue(type, out var service))
            return service as T;

        Debug.LogError($"Service {type.Name} not found");
        return null;
    }
    public static void Clear() 
    {
        m_services.Clear();
    }
}
