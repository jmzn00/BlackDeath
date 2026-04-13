using UnityEngine;

public class ActorComponentBase : MonoBehaviour, IActorComponent 
{
    protected Actor m_actor;
    public virtual void SetInputSource(IInputSource source) 
    {
    
    }
    public virtual void LoadData(ActorSaveData data) { }
    public virtual void SaveData(ActorSaveData data) { }
    public virtual bool Initialize(GameManager game) { return true; }
    public virtual void OnActorComponentsInitialized(Actor actor) { }
    public virtual bool Dispose() { return true; }
}