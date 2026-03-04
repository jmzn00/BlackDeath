using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryComponent : MonoBehaviour, IActorComponent
{
    public event Action<string[]> OnItemsChanged;
    public event Action<string> OnItemAdded;
    public string[] InventoryItems => m_inventoryItems.ToArray();
    private List<string> m_inventoryItems = new List<string>();

    private ItemManager m_itemManager;
    public bool Initialize(GameManager game) 
    {
        m_itemManager = game.Resolve<ItemManager>();
        return true;
    }
    public void OnActorComponentsInitialized(Actor actor)
    {
        // Subscribe to health changes or other events here
    }
    public bool Dispose() 
    {
        return true;
    }
    public void LoadData(ActorSaveData data) 
    {
        string[] inventoryItems = data.InventoryItems ?? new string[0];

        m_inventoryItems = inventoryItems.ToList<string>();

        OnItemsChanged?.Invoke(m_inventoryItems.ToArray());
    }
    public void SaveData(ActorSaveData data) 
    {
        data.InventoryItems = InventoryItems;
    }
    public void SetInputSource(IInputSource source) 
    {
        
    }
    public bool TryAddItem(string itemID) 
    {
        if (string.IsNullOrEmpty(itemID)) 
        {
            return false;
        }
        ItemDefinition itemDef = m_itemManager.GetItemDefinition(itemID);                
        m_inventoryItems.Add(itemID);
        OnItemAdded?.Invoke(itemID);

        return true;
    }
    public bool TryEquipItem() 
    {
        return true;
    }
}
