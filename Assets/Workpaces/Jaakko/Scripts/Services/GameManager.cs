using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public enum GameState 
{
    None,
    Combat
}
public class GameManager : IManager
{
    private Container m_container;
    private List<IManager> m_managers = new();

    private GameState m_state;
    public GameState State => m_state;

    public void SetState(GameState state) 
    {
        if (state == m_state) return;

        m_state = state;
    }

    public bool Init() 
    {
        m_container = new Container();

        m_container.RegisterInstance<GameManager>(this);

        m_container.Register<InputManager>();
        m_container.Register<SaveManager>();
        m_container.Register<ActorManager>();
        m_container.Register<ItemManager>();
        m_container.Register<CombatManager>();
        m_container.Register<CameraManager>();

        // Exclude the GameManager
        m_managers = m_container
            .GetAll<IManager>()
            .Where(m => !(m is GameManager))
            .ToList();

        InitManagers();
        PopulateServices();
        return true;
    }
    public bool Dispose() 
    {
        DisposeManagers();
        Services.Clear();

        return true;
    }
    public void Update(float dt) 
    {
        for (int i = 0; i < m_managers.Count; i++)
            m_managers[i].Update(dt);
    }
    private void InitManagers() 
    {
        foreach (var m in m_managers) 
        {
            if (!m.Init()) 
            {
                Debug.LogError($"Manager {m} failed to Init");
            }
        }
        OnManagersInitialzied(); 
    }
    public void OnManagersInitialzied()
    {
        foreach (var m in m_managers)
            m.OnManagersInitialzied();

        // TEMP
        var ui = GameObject.FindFirstObjectByType<UIController>();
        if (ui)
            ui.Inject(this);
    }
    private void DisposeManagers() 
    {
        foreach (var m in m_managers) 
        {
            if (!m.Dispose()) 
            {
                Debug.LogError($"Manager {m} failed to Dispose");
            }
        }
            
    }    
    private void PopulateServices() 
    {
        Services.Register(this);

        foreach (var m in m_managers) 
        {
            Services.Register(m);
        }      
    }
    public T Resolve<T>() where T : class
    {
        return m_container.Resolve<T>();
    }
}
