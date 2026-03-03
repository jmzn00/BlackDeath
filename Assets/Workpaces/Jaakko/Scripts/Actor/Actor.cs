using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class Actor : MonoBehaviour, IActor
{
    public string ActorID => m_actorID;
    [SerializeField] private string m_actorID;

    private Dictionary<Type, IActorComponent> m_components = new();

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying) return;

        if (string.IsNullOrEmpty(m_actorID) || IsDuplicateID())
        {
            m_actorID = Guid.NewGuid().ToString();
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }

    private bool IsDuplicateID()
    {
        var actors = FindObjectsByType<Actor>(FindObjectsSortMode.None);

        foreach (var actor in actors)
        {
            if (actor == this) continue;
            if (actor.m_actorID == m_actorID) return true;
        }

        return false;
    }
#endif

    public void EnsureID()
    {
        if (string.IsNullOrEmpty(m_actorID) || IsDuplicateID())
            m_actorID = Guid.NewGuid().ToString();
    }

    public ActorSaveData Save()
    {
        ActorSaveData data = new ActorSaveData()
        {
            ActorID = ActorID,
            Position = transform.position
        };
        SaveActorComponentData(data);
        return data;
    }

    public virtual void LoadData(ActorSaveData data)
    {
        m_actorID = data.ActorID;
        transform.position = data.Position;
        LoadActorComponentData(data);
    }

    public virtual void Init(GameManager game)
    {
        // Get all IActorComponent scripts on this GameObject
        var components = GetComponents<IActorComponent>();
        foreach (var comp in components)
        {
            comp.Initialize(game);
            m_components[comp.GetType()] = comp;
        }

        // Notify all components
        foreach (var comp in m_components.Values)
            comp.OnActorComponentsInitialized(this);
    }

    public virtual void Dispose()
    {
        foreach (var comp in m_components.Values)
            comp.Dispose();

        m_components.Clear();
    }

    private void LoadActorComponentData(ActorSaveData data)
    {
        foreach (var comp in m_components.Values)
            comp.LoadData(data);
    }

    private void SaveActorComponentData(ActorSaveData data)
    {
        foreach (var comp in m_components.Values)
            comp.SaveData(data);
    }

    // Dynamically add a component
    public T AddComponent<T>() where T : Component, IActorComponent
    {
        var comp = gameObject.AddComponent<T>();
        m_components[typeof(T)] = comp;
        return comp;
    }

    // Get a component by generic type
    public T Get<T>() where T : class, IActorComponent
    {
        if (m_components.TryGetValue(typeof(T), out var comp))
            return comp as T;
        return null;
    }

    // Get a component by Type
    public IActorComponent Get(Type type)
    {
        if (m_components.TryGetValue(type, out var comp))
            return comp;
        return null;
    }
}