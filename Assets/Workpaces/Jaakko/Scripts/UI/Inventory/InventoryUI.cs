[UIFor(typeof(InventoryComponent), typeof(InventoryView))]
public class InventoryUI : UIComponentBase
{
    private InventoryView m_view;
    private InventoryComponent m_inventory;
    public InventoryUI(InventoryComponent component, InventoryView view)
    {
        m_view = view;
        m_inventory = component;
    }
    public override void Initialize()
    {
        m_inventory.OnItemsChanged += m_view.OnInventoryItemsChanged;
    }
    public override void Dispose()
    {
        m_inventory.OnItemsChanged -= m_view.OnInventoryItemsChanged;
    }
    public override void Toggle(bool show)
    {
        if (show)
            m_view.View();
        else
            m_view.Hide();
    }
}
