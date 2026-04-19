using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class Actor : MonoBehaviour, IActor
{
    public string ActorID => m_actorID;
    [SerializeField] private string m_actorID;

    private Dictionary<Type, IActorComponent> m_components = new();

    [SerializeField] private Team m_team = Team.Neutral;
    [SerializeField] private ControlType m_controlType = ControlType.Ai;

    public Team Team => m_team;
    public ControlType ControlType => m_controlType;

    private PlayerInputSource m_playerInputSource;
    private AIInputSource m_aiInputSource;

    [SerializeField] private Transform m_trackingTarget; // TEMP
    public Transform TrackingTarget => m_trackingTarget;
    public Sprite actorSprite; // TEMP

    protected GameManager m_game;
    public GameManager Game => m_game;

    private IInputSource m_inputSource;
    public IInputSource InputSource => m_inputSource;

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
    private void OnControlChanged(Actor actor) 
    {
        if (actor == this) 
        {
            ChangeComponentInputSource(m_playerInputSource);
        }
        else 
        {
            m_aiInputSource.SetTarget(actor.transform);
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
#if UNITY_EDITOR
        if (string.IsNullOrEmpty(m_actorID) || IsDuplicateID())
            m_actorID = Guid.NewGuid().ToString();
#endif
    }
    protected virtual void GameStateChanged(GameState state)
    {

    }

    public ActorSaveData Save()
    {
        ActorSaveData data = new ActorSaveData()
        {
            ActorID = ActorID,
            Position = transform.position,
            ActorName = name
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
    private ActorManager m_actorManager;
    public virtual void Init(GameManager game)
    {
        m_game = game;
        m_game.OnStateChanged += GameStateChanged;

        var components = GetComponents<IActorComponent>();         
        foreach (var comp in components)
        {
            comp.Initialize(game);
            m_components[comp.GetType()] = comp;
        }
        var childComponents = GetComponentsInChildren<IActorComponent>();
        foreach (var comp in childComponents) 
        {
            comp.Initialize(game);
            m_components[comp.GetType()] = comp;
        }

        ActorComponentsInitialized();

        m_playerInputSource = new PlayerInputSource(game.Resolve<InputManager>());
        m_aiInputSource = new AIInputSource(transform);
        m_actorManager = game.Resolve<ActorManager>();
        m_actorManager.OnActorControlChanged += OnControlChanged;
    }
    public virtual void ActorComponentsInitialized()
    {
        foreach (var comp in m_components.Values)
            comp.OnActorComponentsInitialized(this);
    }
    public virtual void Dispose()
    {
        m_game.OnStateChanged -= GameStateChanged;
        m_actorManager.OnActorControlChanged -= OnControlChanged;

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
    public T AddComponent<T>() where T : Component, IActorComponent
    {
        var comp = gameObject.AddComponent<T>();
        m_components[typeof(T)] = comp;
        return comp;
    }
    public T Get<T>() where T : class, IActorComponent
    {
        if (m_components.TryGetValue(typeof(T), out var comp))
            return comp as T;

        foreach (var kv in m_components)
        {
            if (kv.Value is T matched)
                return matched;
        }

        return null;
    }
}