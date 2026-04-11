using System;
using UnityEngine;
using UnityEngine.UI;

public class LoadView : UIViewBase
{
    [SerializeField] private Button m_backButton;

    [SerializeField] private Transform m_slotAnchor;
        
    [SerializeField] private SaveSlotButton m_slotPrefab;

    public event Action<MainMenuState> OnButtonClicked;
    public override void Init() 
    {
        ToggleButton(m_backButton, true);

        m_backButton.onClick.AddListener(() =>
        {
            OnButtonClicked?.Invoke(MainMenuState.Main);
        });
    }
    public SaveSlotButton CreateSlot(SaveSlotMeta meta) 
    {
        SaveSlotButton b = Instantiate(m_slotPrefab, m_slotAnchor);
        Button button = b.GetComponentInChildren<Button>();

        ToggleButton(button, true);

        return b;
    }
}
