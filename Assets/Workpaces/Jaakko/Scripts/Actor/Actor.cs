using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class Actor : MonoBehaviour, IActor
{
    public string ActorID => m_actorID;
    [SerializeField] private string m_actorID;

    private List<IActorComponent> m_actorComponents;
    private InventoryComponent m_inventoryComponent;
    public InventoryComponent Inventory => m_inventoryComponent;

    private HealthComponent m_healthComponent;
    public HealthComponent Health => m_healthComponent;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying) return;

        if (string.IsNullOrEmpty(m_actorID) || IsDuplicateID()) 
        {
            m_actorID = Guid.NewGuid().ToString();
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
    private bool IsDuplicateID()
    {
        var actors = FindObjectsByType<Actor>(FindObjectsSortMode.None);

        foreach (var actor in actors)
        {
            if (actor == this)
                continue;

            if (actor.m_actorID == m_actorID)
                return true;
        }
        return false;
    }
#endif

    public void EnsureID() 
    {
        if (string.IsNullOrEmpty(m_actorID) || IsDuplicateID())
            m_actorID = Guid.NewGuid().ToString();
    }
    public ActorSaveData Save() 
    {
        ActorSaveData data = new ActorSaveData()
        {
            ActorID = ActorID
        };  
        SaveActorComponentData(data);
        return data;
    }
    public virtual void LoadData(ActorSaveData data)
    {
        m_actorID = data.ActorID;
        transform.position = data.Position;

        LoadActorComponentData(data);
    }
    public virtual void Init(GameManager game) 
    {       
        m_inventoryComponent = gameObject.AddComponent<InventoryComponent>();
        m_healthComponent = gameObject.AddComponent<HealthComponent>();

        InitializeActorComponents(game);            
    }
    public virtual void Dispose() 
    {
        foreach (var component in m_actorComponents) 
        {
            component.Dispose();
        }
        m_actorComponents?.Clear();
    }    
    private void InitializeActorComponents(GameManager game) 
    {
        m_actorComponents = new List<IActorComponent>(GetComponents<IActorComponent>());
        if (m_actorComponents.Count == 0) return;

        for (int i = 0; i < m_actorComponents.Count; i++) 
        {
            m_actorComponents[i].Initialize(game);
        }
    }
    private void LoadActorComponentData(ActorSaveData data) 
    {
        if (m_actorComponents.Count == 0) return;
        for (int i = 0; i < m_actorComponents.Count; i++)
        {
            m_actorComponents[i].LoadData(data);
        }
    }
    private void SaveActorComponentData(ActorSaveData data) 
    {
        if (m_actorComponents.Count == 0) return;
        for (int i = 0; i < m_actorComponents.Count; i++) 
        {
            m_actorComponents[i].SaveData(data);
        }
    }
}
