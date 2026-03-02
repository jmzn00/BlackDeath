using System;
using UnityEngine;
using UnityEngine.Events;

[ExecuteAlways]
public class Actor : MonoBehaviour, IActor
{
    public string ActorID => m_actorID;
    [SerializeField] private string m_actorID;

    public event Action<bool> OnActorInit;

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
    public virtual ActorSaveData Save() 
    {
        return new ActorSaveData()
        {
            ActorID = ActorID,
            Position = transform.position
        };
    }
    public virtual void Load(ActorSaveData data)
    {
        m_actorID = data.ActorID;
        transform.position = data.Position;
    }
    public virtual void Init() 
    {
        OnActorInit?.Invoke(true);
    }
    public virtual void Dispose() 
    {
        OnActorInit?.Invoke(false);
    }    
}
