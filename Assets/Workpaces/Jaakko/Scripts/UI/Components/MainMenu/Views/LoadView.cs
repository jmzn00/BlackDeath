using System;
using UnityEngine;
using UnityEngine.UI;

public class LoadView : MonoBehaviour, IUIComponentView
{
    [SerializeField] private Button m_backButton;

    [SerializeField] private Transform m_slotAnchor;
        
    [SerializeField] private SaveSlotButton m_slotPrefab;

    public event Action<MainMenuState> OnButtonClicked;
    public void Init() 
    {
        m_backButton.onClick.AddListener(() =>
        {
            OnButtonClicked?.Invoke(MainMenuState.Main);
        });
    }
    public void View() { gameObject.SetActive(true); }
    public void Hide() { gameObject.SetActive(false); }
    public SaveSlotButton CreateSlot(SaveSlotMeta meta) 
    {
        return Instantiate(m_slotPrefab, m_slotAnchor);
    }
}
