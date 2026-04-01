using UnityEngine;

public class LoadView : MonoBehaviour, IUIComponentView
{
    [SerializeField] private SaveSlotButton m_slotPrefab;
    public void Init() 
    {
        
    }
    public void View() { gameObject.SetActive(true); }
    public void Hide() { gameObject.SetActive(false); }
    public SaveSlotButton CreateSlot(SaveSlotMeta meta) 
    {
        return Instantiate(m_slotPrefab, transform);
    }
}
