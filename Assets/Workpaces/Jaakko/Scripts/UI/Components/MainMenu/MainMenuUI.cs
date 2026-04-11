using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum MainMenuState
{
    Main,
    Settings,
    Load,
    Save,
    Quit
}
public class MainMenuUI : UIComponentBase<MainMenuGroup> 
{
    private SaveManager m_save;

    private LoadView m_loadView;
    private MainView m_mainView;
    private SettingsView m_settingsView;

    private List<SaveSlotButton> m_slots = new();

    private MainMenuState m_state;
    public MainMenuUI (GameManager game, MainMenuGroup group) : base (game, group) 
    {
        m_save = m_game.Resolve<SaveManager>();
        
        m_loadView = group.Get<LoadView>();
        m_mainView = group.Get<MainView>();
        m_settingsView = group.Get<SettingsView>();
    }        
    public override void Initialize() 
    {
        GameEvents.OnLoadStarted += LoadStarted;
        GameEvents.OnLoadFinished += LoadFinished;

        m_group.InitAll();

        m_mainView.OnButtonClicked += ChangeState;
        m_settingsView.OnButtonClicked += ChangeState;
        m_loadView.OnButtonClicked += ChangeState;

        BuildSaveSlots();
        ChangeState(MainMenuState.Main);
    }
    public override void Dispose() 
    {
        GameEvents.OnLoadStarted -= LoadStarted;
        GameEvents.OnLoadFinished -= LoadFinished;

        m_mainView.OnButtonClicked -= ChangeState;
        m_settingsView.OnButtonClicked -= ChangeState;
        m_loadView.OnButtonClicked -= ChangeState;

        m_group.DisposeAll();

        DestroySaveSlots();
    }
    public override bool IsVisible()
    {
        return m_loadView.gameObject.activeInHierarchy 
            || m_mainView.gameObject.activeInHierarchy;    
    }
    public override void Toggle(bool show)
    {
        if (show) 
        {
            m_mainView.View();
        }
        else 
        {
            m_group.HideAll();
        }
        base.Toggle(show);
    }
    private void LoadStarted() 
    {
        m_mainView.LoadStarted();
    }
    private void LoadFinished() 
    {
        m_mainView.LoadFinished();
    }
    public override void SceneChanged(SceneData data)
    {
        m_mainView.SceneChanged(data);
    }    
    private void BuildSaveSlots() 
    {
        List<SaveSlotMeta> slots = m_save.GetAllSlots();
        for (int i = 0; i < slots.Count; i++)
        {
            SaveSlotMeta meta = slots[i];

            SaveSlotButton s = m_loadView.CreateSlot(meta);
            s.Bind(meta, i);
            s.OnPressed += Load;

            m_slots.Add(s);
        }
    }
    private void UpdateSaveSlots() 
    {
        for (int i = 0; i < m_slots.Count; i++) 
        {
            SaveSlotButton s = m_slots[i];
            s.OnPressed -= Load;
        }

        List<SaveSlotMeta> metas = m_save.GetAllSlots();
        for (int i = 0; i < metas.Count; i++) 
        {
            m_slots[i].Bind(metas[i], i);
            m_slots[i].OnPressed += Load;
        }
    }
    private void DestroySaveSlots() 
    {
        foreach (var s in m_slots) 
        {
            s.OnPressed -= Load;
            GameObject.Destroy(s.gameObject);
        }            
        m_slots.Clear();
    }
    private void Save(int index) 
    {
        m_game.SaveGame(index);

        UpdateSaveSlots();
    }
    private void Load(SaveSlotMeta meta, int index) 
    {
        if (!meta.HasData) return;

        if (m_state == MainMenuState.Save) 
        {
            Save(index);
        }
        else 
        {
            m_game.LoadGame(meta);
        }        
    }    
    private void ChangeState(MainMenuState state) 
    {
        m_group.HideAll();

        m_state = state;
        switch (state)
        {
            case MainMenuState.Main:
                m_mainView.View();
                break;
            case MainMenuState.Settings:
                m_settingsView.View();
                break;
            case MainMenuState.Load:
                m_loadView.View();
                break;
            case MainMenuState.Save:
                m_loadView.View();
                break;
            case MainMenuState.Quit:
                
                break;
        }
        UpdateNavigation();
    }
}