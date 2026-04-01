using System.Collections.Generic;
using UnityEngine;

public enum MainMenuState
{
    Main,
    Settings,
    Start,
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
        
        m_loadView = group.LoadView;
        m_mainView = group.MainView;
        m_settingsView = group.SettingsView;

        m_mainView.OnButtonClicked += ChangeState;
        m_settingsView.OnButtonClicked += ChangeState;
    }    
    public override void Initialize() 
    {
        m_loadView.Init();
        m_mainView.Init();
        m_settingsView.Init();

        BuildSaveSlots();

        ChangeState(MainMenuState.Main);
    }
    public override void Dispose() 
    {
        m_mainView.OnButtonClicked -= ChangeState;
        m_settingsView.OnButtonClicked -= ChangeState;

        foreach (var s in m_slots) 
        {
            GameObject.Destroy(s.gameObject);
        }
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
            m_mainView.Hide();
            m_loadView.Hide();
            m_settingsView.Hide();
        }
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
    private void DestroySaveSlots() 
    {
        foreach (var s in m_slots)
            GameObject.Destroy(s.gameObject);

        m_slots.Clear();
    }
    private void Save(int index) 
    {
        m_game.SaveGame(index);

        DestroySaveSlots();
        BuildSaveSlots();
    }
    private void Load(SaveSlotMeta meta, int index) 
    {
        if (m_state == MainMenuState.Save) 
        {
            Save(index);
        }
        if (meta.HasData) 
        {
            m_game.LoadGame(meta);
        }        
    }

    private void ChangeState(MainMenuState state) 
    {
        HideViews();

        m_state = state;
        switch (state)
        {
            case MainMenuState.Main:
                m_mainView.View();
                break;
            case MainMenuState.Settings:
                m_settingsView.View();
                break;
            case MainMenuState.Start:
                m_loadView.View();
                break;
            case MainMenuState.Save:
                m_loadView.View();
                break;
            case MainMenuState.Quit:
                
                break;
        }
    }
    private void HideViews() 
    {
        m_mainView.Hide();
        m_loadView.Hide();
        m_settingsView.Hide();
    }
}