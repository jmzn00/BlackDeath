using System.Collections.Generic;
using UnityEngine;

public class ItemManager : IManager
{
    private Dictionary<string, ItemDefinition> m_itemDatabase = new();

    public ItemManager() 
    {
        
    }
    public bool Init() 
    {
        ItemDefinition[] items = Resources.LoadAll<ItemDefinition>("Items");

        foreach (ItemDefinition item in items) 
        {
            if (!m_itemDatabase.ContainsKey(item.ItemID))
                m_itemDatabase.Add(item.ItemID, item);
            else
                Debug.LogWarning($"Duplicate ItemID detected: {item.ItemID}");
        }
        return true;
    }
    public void OnManagersInitialzied()
    {

    }
    public bool Dispose() 
    {
        m_itemDatabase.Clear();
        return true;
    }
    public void Update(float dt)
    {

    }
    public List<ItemDefinition> GetAllItems()
    {
        return new List<ItemDefinition>(m_itemDatabase.Values);
    }
    public ItemDefinition GetItemDefinition(string itemID) 
    {
        m_itemDatabase.TryGetValue(itemID, out ItemDefinition item);
        return item;
    }
}
