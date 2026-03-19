using UnityEngine;
public abstract class ActorStatusEffect : ScriptableObject
{
    public int duration = 1;
    public string displayName;
    [SerializeField] private bool m_isStackable = false;
    [TextArea(4, 10)]
    public string description;

    public CombatActor Owner {  get; private set; }
    public int RemainingTurns { get; protected set; }
    public bool IsStackable => m_isStackable;
    public void AddDuration(int amount) 
    {
        RemainingTurns += amount;
    }
    public void Initialize(CombatActor owner) 
    {
        Owner = owner;
        RemainingTurns = duration;
        OnApply();
    }
    public void TurnStart() 
    {
        OnTurnStart();
    }
    public void TurnEnd() 
    {
        OnTurnEnd();
    }
    public bool TickDuration()
    {
        RemainingTurns--;
        return RemainingTurns <= 0;
    }
    protected virtual void OnApply() { }
    protected virtual void OnTurnStart() { }
    protected virtual void OnTurnEnd() { }
    protected virtual void OnExpire() { }    
    public void Expire() 
    {
        OnExpire();
    }
    
}
