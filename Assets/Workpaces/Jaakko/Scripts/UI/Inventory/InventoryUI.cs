using System;

[UIFor(typeof(InventoryComponent), typeof(InventoryView))]
public class InventoryUI : UIComponentBase
{
    private InventoryView m_view;
    private InventoryComponent m_inventory;
    private Action<string> m_onItemAddedHandler;
    public InventoryUI(InventoryComponent component, InventoryView view)
    {
        m_view = view;
        m_inventory = component;
    }
    public override void Initialize(Actor actor)
    {
        m_inventory = actor.Get<InventoryComponent>();
        m_view.Init(actor);

        SubscribeInventory();
        m_view.OnInventoryItemsChanged(m_inventory.InventoryItems);
    }
    public override void Dispose()
    {
        UnsubscribeInventory();
    }    
    private void SubscribeInventory()
    {
        if (m_inventory != null)
        {
            m_inventory.OnItemsChanged += m_view.OnInventoryItemsChanged;

            m_onItemAddedHandler = item => m_view.OnInventoryItemsChanged(m_inventory.InventoryItems);
            m_inventory.OnItemAdded += m_onItemAddedHandler;
        }
    }
    private void UnsubscribeInventory()
    {
        if (m_inventory != null)
        {
            m_inventory.OnItemsChanged -= m_view.OnInventoryItemsChanged;

            if (m_onItemAddedHandler != null)
            {
                m_inventory.OnItemAdded -= m_onItemAddedHandler;
                m_onItemAddedHandler = null;
            }
        }
    }
    public override void OnActorChanged(Actor actor)
    {
        UnsubscribeInventory();

        m_inventory = actor.Get<InventoryComponent>();
        m_view.OnActorChanged(actor);

        SubscribeInventory();
        m_view.OnInventoryItemsChanged(m_inventory.InventoryItems);
    }
    public override void Toggle(bool show)
    {
        if (show)
            m_view.View();
        else
            m_view.Hide();
    }
}
