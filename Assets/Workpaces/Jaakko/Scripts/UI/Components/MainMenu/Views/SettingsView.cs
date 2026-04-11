using System;
using UnityEngine;
using UnityEngine.UI;

public class SettingsView : UIViewBase
{
    [SerializeField] private Button m_backButton;

    public event Action<MainMenuState> OnButtonClicked;

    public override void Init() 
    {
        ToggleButton(m_backButton, true);

        m_backButton.onClick.AddListener(() =>
        {
            OnButtonClicked?.Invoke(MainMenuState.Main);
        });
    }
}