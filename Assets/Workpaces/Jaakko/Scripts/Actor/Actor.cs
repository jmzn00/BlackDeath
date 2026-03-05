using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

[ExecuteAlways]
public class Actor : MonoBehaviour, IActor
{
    public string ActorID => m_actorID;
    [SerializeField] private string m_actorID;

    private Dictionary<Type, IActorComponent> m_components = new();

    [SerializeField] private bool m_playable;
    public bool IsPlayable => m_playable;
    public bool IsControlled { get; private set; }

    private PlayerInputSource m_playerInputSource;
    private AIInputSource m_aiInputSource;

    private CinemachineCamera m_camera; // TEMP
    [SerializeField] private Transform m_trackingTarget; // TEMP
    public Sprite actorSprite; // TEMP

    private UIController m_uiController;
    public UIController UI => m_uiController;

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
    public void SetControl(bool controlled) 
    {
        if (IsControlled == controlled)
            return;

        IsControlled = controlled;

        if (controlled) 
        {
            ChangeComponentInputSource(m_playerInputSource);
            m_camera.Follow = m_trackingTarget; // TEMP
            m_uiController.ChangeActor(this); // TEMP
        }
        else 
        {
            ChangeComponentInputSource(m_aiInputSource);
        }
    }
    void ChangeComponentInputSource(IInputSource source) 
    {
        foreach (var comp in m_components.Values)
        {
            comp.SetInputSource(source);
        }
    }
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
        m_camera = FindFirstObjectByType<CinemachineCamera>(); // TEMP, for testing purposes

        if (m_playable) 
        {
            m_uiController = FindFirstObjectByType<UIController>(); // TEMP, for testing purposes
        }
            

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

        m_playerInputSource = new PlayerInputSource(game.Resolve<InputManager>());
        m_aiInputSource = new AIInputSource();
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