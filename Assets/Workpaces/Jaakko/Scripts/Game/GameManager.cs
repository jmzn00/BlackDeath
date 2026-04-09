using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
public enum GameState 
{
    None,
    Combat,
    Dialogue
}
public class GameManager : ManagerBase
{
    private Container m_container;

    private int m_managersReady;
    private List<IManager> m_managers = new();

    private GameState m_state;
    public GameState State => m_state;
    public event Action<GameState> OnStateChanged;

    public void SetState(GameState state) 
    {
        if (state == m_state) return;

        m_state = state;
        OnStateChanged?.Invoke(state);
    }
    public override bool Init() 
    {
        m_container = new Container();

        m_container.RegisterInstance<GameManager>(this);

        m_container.Register<InputManager>();
        m_container.Register<SaveManager>();
        m_container.Register<ActorManager>();
        m_container.Register<ItemManager>();
        m_container.Register<CombatManager>();
        m_container.Register<UIManager>();
        m_container.Register<CameraManager>();
        m_container.Register<DialogueManager>();
        m_container.Register<AudioManager>();
        m_container.Register<PoolManager>();

        m_managers = m_container
            .GetAll<IManager>()
            .Where(m => !(m is GameManager))
            .ToList();

        InitManagers();
        PopulateServices();

        SceneManager.sceneLoaded += SceneLoaded;
        return true;
    }
    public override bool Dispose() 
    {
        DisposeManagers();
        Services.Clear();

        SceneManager.sceneLoaded -= SceneLoaded;
        return true;
    }
    public override void Update(float dt) 
    {
        for (int i = 0; i < m_managers.Count; i++)
            m_managers[i].Update(dt);
    }
    public void SaveGame(int slot) 
    {
        
        var save = Resolve<SaveManager>();

        SceneData data = new SceneData(SceneManager.GetActiveScene().name,
            true);

        save.Save(slot, data);
    }
    public void LoadGame(SaveSlotMeta meta) 
    {
        foreach (var m in m_managers)
            m.OnSceneUnloaded();

        GameEvents.LoadStarted();

        var save = Resolve<SaveManager>();
        save.SetCurrentSlot(meta.Slot);

        SceneManager.LoadScene(meta.SceneName);
    }
    private void SceneLoaded(Scene scene, LoadSceneMode mode) 
    {
        m_managersReady = 0;


        bool isGame = true;
        // not type safe but works for now
        if (scene.name == "Scene_MainMenu") 
        {
            GameEvents.LoadFinished();
            isGame = false;
        }
        SceneData data = new SceneData(scene.name, isGame);

        foreach (var m in m_managers)
            m.OnSceneLoaded(data);

        Resolve<SaveManager>().RestoreAfterSceneLoad();
    }    
    private void InitManagers() 
    {
        foreach (var m in m_managers) 
        {
            if (!m.Init()) 
            {
                Debug.LogError($"Manager {m} failed to Init");
            }
            else 
            {
                m.OnReady += ManagerReady;
            }
        }
        ManagersInitialzied(); 
    }
    private void ManagerReady() 
    {
        m_managersReady++;

        if (m_managersReady >= m_managers.Count) 
        {
            GameEvents.LoadFinished();
        }
            
    }
    public void ManagersInitialzied()
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
            else 
            {
                m.OnReady -= ManagerReady;
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
