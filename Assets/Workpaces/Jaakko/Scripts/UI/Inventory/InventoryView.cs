using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryView : MonoBehaviour, IUIComponentView
{
    [Header("Actor")]
    [SerializeField] private Image m_actorImage;
    [SerializeField] private TMP_Text m_actorNameText;
    [SerializeField] private TMP_Text m_inventoryItemsText;

    [Header("Panel")]
    [SerializeField] private Button m_closeButton;

    private InventoryComponent m_inventory;
    public void Init(Actor actor) 
    {
        m_inventory = actor.Get<InventoryComponent>();
        OnActorChanged(actor);
    }    
    public void OnActorChanged(Actor actor) 
    {
        if (actor.actorSprite != null) 
        {
            m_actorImage.sprite = actor.actorSprite;
        }
        m_actorNameText.text = actor.name;
        if (m_inventory != null) 
        {
            m_inventory.OnItemsChanged -= OnInventoryItemsChanged;
            m_inventory = null;
        }
        m_inventory = actor.Get<InventoryComponent>();        
        m_inventory.OnItemsChanged += OnInventoryItemsChanged;
        OnInventoryItemsChanged(m_inventory.InventoryItems);
    }
    public void OnInventoryItemsChanged(string[] text) 
    {
        m_inventoryItemsText.text = "";

        foreach (string item in text) 
        {
            ItemDefinition def = Services.Get<ItemManager>().GetItemDefinition(item);
            m_inventoryItemsText.text += def.DisplayName + "\n";
        }
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
