using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryView : MonoBehaviour, IUIComponentView
{
    [Header("Actor")]
    [SerializeField] private Image m_actorImage;
    [SerializeField] private TMP_Text m_actorNameText;

    public void Init(Actor actor) 
    {
        Debug.Log($"Initializing InventoryView for actor {actor.name}");
        m_actorNameText.text = actor.name;
    }
    public void View() 
    {

    }
    public void Hide() 
    {

    }
    public void OnInventoryItemsChanged(string text) 
    {
        
    }
}
