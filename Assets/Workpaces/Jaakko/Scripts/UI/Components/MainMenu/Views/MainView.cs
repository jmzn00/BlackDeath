using System;
using UnityEngine;
using UnityEngine.UI;

public class MainView : UIViewBase
{
    [Header("Buttons")]
    [SerializeField] private Button m_startButton;
    [SerializeField] private Button m_saveButton;
    [SerializeField] private Button m_settingsButton;
    [SerializeField] private Button m_quitButton;

    [Header("Panels")]
    [SerializeField] private GameObject m_loadPanel;
    
    public event Action<MainMenuState> OnButtonClicked;

    public override void Init() 
    {
        base.Init();

        RegisterSelectable(m_startButton);
        RegisterSelectable(m_saveButton);
        RegisterSelectable(m_settingsButton);
        RegisterSelectable(m_quitButton);


        m_startButton.onClick.AddListener(() =>
        {
            OnButtonClicked?.Invoke(MainMenuState.Load);
        });
        m_saveButton.onClick.AddListener(() =>
        {
            OnButtonClicked?.Invoke(MainMenuState.Save);
        });
        m_settingsButton.onClick.AddListener(() =>
        {
            OnButtonClicked?.Invoke(MainMenuState.Settings);
        });
        m_quitButton.onClick.AddListener(() =>
        {
            OnButtonClicked?.Invoke(MainMenuState.Quit);
        });        
    }
    public void SceneChanged(SceneData sceneData) 
    {
        if (sceneData.IsGameplay) 
        {
            ToggleButton(m_saveButton, true);
        }
        else 
        {
            ToggleButton(m_saveButton, false);
        }
    }
    public void LoadStarted() 
    {
        m_loadPanel.SetActive(true);
    }
    public void LoadFinished() 
    {
        m_loadPanel.SetActive(false);
    }
}
