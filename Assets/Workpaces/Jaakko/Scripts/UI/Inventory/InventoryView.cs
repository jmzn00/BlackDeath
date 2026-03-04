using TMPro;
using UnityEngine;

public class InventoryView : MonoBehaviour, IUIComponentView
{
    [SerializeField] private TMP_Text m_itemListText;
    public void Init() 
    {

    }
    public void View() 
    {

    }
    public void Hide() 
    {

    }
    public void OnInventoryItemsChanged(string text) 
    {
        m_itemListText.text += text + "\n";
    }
}
