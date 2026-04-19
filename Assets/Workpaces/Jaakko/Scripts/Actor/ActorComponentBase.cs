using UnityEngine;

public class ActorComponentBase : MonoBehaviour, IActorComponent 
{
    protected Actor m_actor;
    protected IInputSource m_input;
    public virtual void SetInputSource(IInputSource source) 
    {
        m_input = source;
    }
    public virtual void LoadData(ActorSaveData data) { }
    public virtual void SaveData(ActorSaveData data) { }
    public virtual bool Initialize(GameManager game) { return true; }
    public virtual void OnActorComponentsInitialized(Actor actor) { }
    public virtual bool Dispose() { return true; }
}