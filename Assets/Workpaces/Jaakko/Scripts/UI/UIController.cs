using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using System.Collections.Generic;

public class UIController : MonoBehaviour
{
    private Actor m_player;    
    private List<IUIComponent> m_uiComponents = new List<IUIComponent>();
    public void Init(Actor player) 
    {
        m_player = player;

        BuildModules();
        InitializeModules();

        Debug.Log($"UI Modules Count {m_uiComponents.Count}");
    }
    void BuildModules() 
    {
        var uiTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => typeof(UIComponentBase)
            .IsAssignableFrom(t) && !t.IsAbstract);

        foreach (var type in uiTypes) 
        {
            var attr = type.GetCustomAttribute<UIForAttribute>();
            if (attr == null) continue;

            var actorComponent = m_player.Get(attr.ActorComponentType);
            if (actorComponent == null) 
            {
                Debug.LogWarning($"Actor component of type {attr.ActorComponentType.Name} not found for UI component {type.Name}");
                continue;
            }

            var view = GetComponentInChildren(attr.ViewType, true);
            if (view == null) 
            {
                Debug.LogWarning($"Could not find view {attr.ViewType.Name} in children");
                continue;
            }

            var instance = Activator.CreateInstance(type, actorComponent, view) as IUIComponent;
            if (instance != null)
                m_uiComponents.Add(instance);
        }

    }
    void InitializeModules() 
    {
        foreach (var comp in m_uiComponents)
            comp.Initialize();
    }  
}
