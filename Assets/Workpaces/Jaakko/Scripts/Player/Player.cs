using System;
using UnityEditor;
using UnityEngine;

public class Player : MonoBehaviour, IActor
{
    public string[] InventoryItems;

    public string ActorID => m_actorID;

    [SerializeField] private string m_actorID;

    public void EnsureID() 
    {
        if (string.IsNullOrEmpty(m_actorID))
            m_actorID = Guid.NewGuid().ToString();
    }
    public ActorSaveData Save() 
    {
        return new ActorSaveData()
        {
            ActorID = ActorID,
            Position = transform.position,
            InventoryItems = InventoryItems
        };
    }
    public void Load(ActorSaveData data)
    {
        m_actorID = data.ActorID;
        transform.position = data.Position;
        InventoryItems = data.InventoryItems;
    }
    public void Init() 
    {
        EnsureID();
        Debug.Log("Actor Init");
    }
    public void Dispose() 
    {
        Debug.Log("Actor Dispose");
    }    
}
