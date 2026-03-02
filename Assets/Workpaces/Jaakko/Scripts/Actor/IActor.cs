using System;
using UnityEngine;

[Serializable]
public class ActorSaveData
{
    public string ActorID;
    public Vector3 Position;
    public bool IsDead;
    public string[] InventoryItems;
}

public interface IActor
{
    string ActorID { get; }
    void Init();
    void Dispose();
    void Load(ActorSaveData data);
    void EnsureID();
    ActorSaveData Save();        
}
