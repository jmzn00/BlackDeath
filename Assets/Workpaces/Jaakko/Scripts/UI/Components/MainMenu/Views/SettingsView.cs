using System;
using UnityEngine;
using UnityEngine.UI;

public class SettingsView : MonoBehaviour, IUIComponentView 
{
    [SerializeField] private Button m_backButton;

    public event Action<MainMenuState> OnButtonClicked;
    public void View() { gameObject.SetActive(true); }
    public void Hide() { gameObject.SetActive(false); }

    public void Init() 
    {
        m_backButton.onClick.AddListener(() =>
        {
            OnButtonClicked?.Invoke(MainMenuState.Main);
        });
    }
}