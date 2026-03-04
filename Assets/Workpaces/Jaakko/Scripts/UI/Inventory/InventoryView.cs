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

    private InventoryComponent m_inventoryComponent;

    private Actor m_currentActor;

    public void Init(Actor actor) 
    {
        m_actorNameText.text = actor.name;
        m_actorImage.sprite = actor.actorSprite;

        m_inventoryComponent = actor.Get<InventoryComponent>();

        m_closeButton.onClick.AddListener(() => Hide());
    }
    public void View() 
    {
        gameObject.SetActive(true);
    }
    public void Hide() 
    {
        gameObject.SetActive(false);
    }
    public void OnActorChanged(Actor actor) 
    {
        m_currentActor = actor;

        m_actorNameText.text = actor.name;
        m_actorImage.sprite = actor.actorSprite;
        m_inventoryComponent = actor.Get<InventoryComponent>();

        OnInventoryItemsChanged(m_inventoryComponent.InventoryItems);
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
}
