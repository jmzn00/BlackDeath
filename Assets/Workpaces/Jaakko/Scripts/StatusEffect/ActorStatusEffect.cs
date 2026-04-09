using System;
using UnityEngine;
public class StatusEffectInstance : IDamageSource
{
    public CombatActor SourceActor { get; }
    public CombatActor Applier { get; private set; }
    public string SourceName { get; }

    public int RemainingTurns { get; private set; }

    private DamageSystem m_damage;
    public readonly ActorStatusEffect Template;

    public event Action<int> OnDurationChanged;

    public StatusEffectInstance(ActorStatusEffect effect, CombatActor owner, CombatActor applier
        , DamageSystem damage) 
    {
        Template = effect;

        SourceActor = owner;
        SourceName = Template.displayName;
        Applier = applier;
        m_damage = damage;

        RemainingTurns = effect.duration;
    }
    public void TurnStart() => Template.OnTurnStart(this);
    public void TurnEnd() => Template.OnTurnEnd(this);
    public void Expire() => Template.OnExpire(this);

    public bool TickDuration() 
    {
        RemainingTurns--;
        OnDurationChanged?.Invoke(RemainingTurns);
        if (Template.damage > 0) 
        {
            ApplyDamage(Template.damage);
        }
        if (Template.heal > 0) 
        {
            ApplyHeal(Template.heal);
        }
        if (RemainingTurns <= 0) 
        {            
            Template.OnExpire(this);
            SourceActor.RemoveEffect(this);
            return true;
        }        
        return false;
    }
    public void UpdateDuration(int amount) 
    {
        RemainingTurns += amount;
    }
    private void ApplyDamage(float amount) 
    {
        m_damage.ApplyDamage(amount, Applier, SourceActor);
    }
    private void ApplyHeal(float amount) 
    {
        m_damage.ApplyHeal(amount, Applier, SourceActor);
    }
    
}
public abstract class ActorStatusEffect : ScriptableObject
{
    [field: SerializeField] public int duration { get; private set; } = 1;
    [field: SerializeField] public float damage { get; private set; } = 0;
    [field: SerializeField] public float heal { get; private set; } = 0;
    [field: SerializeField] public string displayName { get; private set; } = "StatusEffect";
    [field: SerializeField] public bool isStackable { get; private set; } = false;
    [field: SerializeField] public Sprite statusEffectSprite { get; private set; }

    public virtual bool CanPerformAction(CombatAction action, out string reason) 
    {
        reason = "";
        return true;
    }
    public virtual void OnApply(StatusEffectInstance instance) { }
    public virtual void OnTurnStart(StatusEffectInstance instance) { }
    public virtual void OnTurnEnd(StatusEffectInstance instance) { }
    public virtual void OnExpire(StatusEffectInstance instance) { } 
}
