using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : IManager
{
    private Container m_container;
    private List<IManager> m_managers = new();

    public bool Init() 
    {
        m_container = new Container();

        // Register this existing GameManager instance so Container won't create another one
        m_container.RegisterInstance<GameManager>(this);

        m_container.Register<InputManager>();
        m_container.Register<SaveManager>();
        m_container.Register<ActorManager>();
        m_container.Register<ItemManager>();

        // Exclude the GameManager itself from the managed managers list
        m_managers = m_container.GetAll<IManager>().Where(m => !(m is GameManager)).ToList();

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
