using System;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : IManager
{
    private GameManager m_game;
    private AudioController m_controller;
    public AudioController Controller => m_controller;

    private List<IAudioModule> m_modules;

    public event Action OnReady;
    public bool IsReady { get; private set; }
    public AudioManager(GameManager game) 
    {
        m_game = game;
    }
    public bool Init() 
    {
        m_controller = GameObject
            .FindFirstObjectByType<AudioController>();
        
        if (m_controller == null) 
        {
            Debug.LogWarning($"No AudioController found in scene");
            return false;
        }
        m_controller.Inject(this);
        m_game.OnStateChanged += GameStateChanged;    
        return true;
    }
    public bool Dispose() 
    {
        m_game.OnStateChanged -= GameStateChanged;
        return true;
    }
    public void OnManagersInitialzied() 
    {
        m_modules = new()
        {
            new CombatAudioModule(this)
        };
    }
    public void OnSceneLoaded(SceneData data) 
    {
        IsReady = false;
        SetReady();
    }
    public void Update(float dt)
    {
        for (int i = 0; i < m_modules.Count; i++) 
        {
             m_modules[i].Update(dt);
        }
    }
    private void GameStateChanged(GameState state) 
    {
        foreach (var m in m_modules)
            m.Deactivate();

        switch (state) 
        {
            case GameState.Combat:
                GetModule<CombatAudioModule>()?.Activate();
                break;
            case GameState.Dialogue:
                
                break;
            case GameState.None:

                break;
        }
    }
    private T GetModule<T>() where T : class, IAudioModule 
    {
        for (int i = 0; i < m_modules.Count; i++) 
        {
            if (m_modules[i] is T t)
                return t;
        }
        return null;
    }
    private void SetReady() 
    {
        if (IsReady) return;

        IsReady = true;

        OnReady?.Invoke();
    }
}
