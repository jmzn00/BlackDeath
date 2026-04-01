using UnityEngine;

public class LoadView : MonoBehaviour, IUIComponentView
{
    [SerializeField] private SaveSlotButton m_slotPrefab;
    public void Init() 
    {
        
    }
    public void View() { transform.parent.gameObject.SetActive(true); }
    public void Hide() { transform.parent.gameObject.SetActive(false); }
    public SaveSlotButton CreateSlot(SaveSlotMeta meta) 
    {
        return Instantiate(m_slotPrefab, transform);
    }
}
