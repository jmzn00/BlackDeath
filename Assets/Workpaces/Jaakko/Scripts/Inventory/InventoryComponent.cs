using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryComponent : MonoBehaviour, IActorComponent
{
    public string[] InventoryItems => m_inventoryItems.ToArray();
    private List<string> m_inventoryItems = new List<string>();

    private ItemManager m_itemManager;
    public bool Initialize(GameManager game) 
    {
        m_itemManager = game.Resolve<ItemManager>();
        return true;
    }
    public bool Dispose() 
    {
        return true;
    }
    public void LoadData(ActorSaveData data) 
    {
        string[] inventoryItems = data.InventoryItems ?? new string[0];

        m_inventoryItems = inventoryItems.ToList<string>();

        foreach (string item in inventoryItems)
        {
            ItemDefinition def = m_itemManager.GetItemDefinition(item);
            Debug.Log($"{gameObject.name} Loaded {def.DisplayName}");
        }
    }
    public void SaveData(ActorSaveData data) 
    {
        data.InventoryItems = InventoryItems;
    }
    public bool TryAddItem(string itemID) 
    {
        if (string.IsNullOrEmpty(itemID)) 
        {
            return false;
        }
        Debug.Log($"{gameObject.name} Added {m_itemManager.GetItemDefinition(itemID).DisplayName}");
        m_inventoryItems.Add(itemID);
        return true;
    }
}
