using System;
using UnityEngine;
using UnityEngine.UI;

public class MainView : MonoBehaviour, IUIComponentView
{
    [Header("Buttons")]
    [SerializeField] private Button m_startButton;
    [SerializeField] private Button m_saveButton;
    [SerializeField] private Button m_settingsButton;
    [SerializeField] private Button m_quitButton;
    

    public event Action<MainMenuState> OnButtonClicked;
    public void Init() 
    {
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
    public void View() 
    {
        gameObject.SetActive(true); 
    }
    public void Hide() 
    {
        gameObject.SetActive(false);
    }
}
