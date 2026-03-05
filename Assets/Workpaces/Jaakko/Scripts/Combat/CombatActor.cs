using UnityEngine;

[RequireComponent(typeof(Actor))]
public class CombatActor : MonoBehaviour, IActorComponent
{
    public bool IsDead;
    public bool IsPlayer;

    private Actor m_actor;
    public bool Initialize(GameManager game) 
    {
        m_actor = GetComponent<Actor>();

        IsPlayer = m_actor.IsPlayable;
        return true;
    }
    public bool Dispose() 
    {
        return true;
    }
    public void OnActorComponentsInitialized(Actor actor) 
    {
    
    }
    public void SetInputSource(IInputSource source) 
    {
        
    }
    public void LoadData(ActorSaveData data) 
    {
    
    }
    public void SaveData(ActorSaveData data) 
    {
    
    }
}
