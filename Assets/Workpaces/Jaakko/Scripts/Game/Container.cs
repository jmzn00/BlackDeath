using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;

public class Container
{
    private readonly Dictionary<Type, object> m_instances = new();
    private readonly HashSet<Type> m_resolving = new();

    public void Register<T>() where T : class
    {
        Resolve(typeof(T));
    }

    public T Resolve<T>() where T : class
    {
        return (T)Resolve(typeof(T));
    }
    private object Resolve(Type type) 
    {   
        if (typeof(MonoBehaviour).IsAssignableFrom(type)) 
        {
            Debug.LogError($"Cannot resolve MonoBehaviour type {type.FullName} in Container.");
            return null;
        }
        if (m_instances.TryGetValue(type, out var existing))
            return existing;

        ConstructorInfo ctor = type.GetConstructors().First();
        var parameters = ctor.GetParameters().
            Select(p => Resolve(p.ParameterType)).ToArray();

        var instance = Activator.CreateInstance(type, parameters);
        m_instances[type] = instance;

        return instance;
    }
    public IEnumerable<T> GetAll<T>() 
    {
        return m_instances.Values.OfType<T>();
    }
    public void RegisterInstance<T>(T instance) where T : class
    {
        if (instance == null) throw new ArgumentNullException(nameof(instance));
        m_instances[typeof(T)] = instance;
    }
}
