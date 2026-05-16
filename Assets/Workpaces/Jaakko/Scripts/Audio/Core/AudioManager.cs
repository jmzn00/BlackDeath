using System;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : ManagerBase
{
    private GameManager m_game;
    private AudioController m_controller;
    public AudioController Controller => m_controller;

    private List<IAudioModule> m_modules;
    public AudioManager(GameManager game) 
    {
        m_game = game;
    }
    public override bool Init() 
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
    public override bool Dispose() 
    {
        m_game.OnStateChanged -= GameStateChanged;
        return true;
    }
    public override void OnManagersInitialzied()
    {
        m_modules = new()
        {
            new CombatAudioModule(this),
            new MusicModule(this)
        };

        var music = GetModule<MusicModule>();
        music?.Activate();
        music?.PlayForState(GameState.None);
    }
    public override void Update(float dt)
    {
        for (int i = 0; i < m_modules.Count; i++)
        {
             m_modules[i].Update(dt);
        }
    }
    private void GameStateChanged(GameState state)
    {
        // MusicModule.Deactivate() is a no-op — music keeps playing across state changes
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

        // Always crossfade music to the track appropriate for the new state
        GetModule<MusicModule>()?.PlayForState(state);
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
}
