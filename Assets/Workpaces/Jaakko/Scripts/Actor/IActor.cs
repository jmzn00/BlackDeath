using System;
using UnityEngine;

[Serializable]
public class ActorSaveData
{
    public string ActorID;
    public Vector3 Position;

    public string[] InventoryItems;
    public float Health;

    public string DialogueNodeID;
}

public interface IActor
{
    string ActorID { get; }
    void Init(GameManager game);
    void Dispose();
    void LoadData(ActorSaveData data);
    void EnsureID();
    ActorSaveData Save();        
}
