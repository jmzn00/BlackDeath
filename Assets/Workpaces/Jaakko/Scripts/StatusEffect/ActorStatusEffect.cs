using System;
using UnityEngine;
public class StatusEffectInstance : IDamageSource
{
    public CombatActor SourceActor { get; }
    public string SourceName { get; }

    public int RemainingTurns;
    private DamageSystem m_damage;
    public readonly ActorStatusEffect Template;

    public event Action<int> OnDurationChanged;

    public StatusEffectInstance(ActorStatusEffect effect, CombatActor owner
        , DamageSystem damage) 
    {
        Template = effect;

        SourceActor = owner;
        SourceName = Template.displayName;
        m_damage = damage;

        RemainingTurns = effect.duration;
    }
    public void TurnStart() => Template.OnTurnStart(this);
    public void TurnEnd() => Template.OnTurnEnd(this);
    public void Expire() => Template.Expire();

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
        m_damage.ApplyDamage(amount, this, SourceActor);
    }
    private void ApplyHeal(float amount) 
    {
        m_damage.ApplyHeal(amount, this, SourceActor);
    }
    
}
public abstract class ActorStatusEffect : ScriptableObject
{
    public int duration = 1;
    public float damage = 0;
    public float heal = 0;
    public string displayName;
    public bool isStackable = false;

    public CombatActor Owner {  get; private set; }
    public int RemainingTurns { get; protected set; }
    public bool IsStackable => isStackable;

    public Sprite statusEffectSprite;

    public virtual bool CanPerformAction(CombatAction action, out string reason) 
    {
        reason = "";
        return true;
    }
    public virtual void OnApply(StatusEffectInstance instance) { }
    public virtual void OnTurnStart(StatusEffectInstance instance) { }
    public virtual void OnTurnEnd(StatusEffectInstance instance) { }
    public virtual void OnExpire(StatusEffectInstance instance) { }




    public void AddDuration(int amount) 
    {
        RemainingTurns += amount;
    }
    public void Initialize(CombatActor owner) 
    {
        Owner = owner;
        RemainingTurns = duration;
        //OnApply();
    }
    public void TurnStart() 
    {
        //OnTurnStart();
    }
    public void TurnEnd() 
    {
        //OnTurnEnd();
    }
    public bool TickDuration()
    {
        RemainingTurns--;
        return RemainingTurns <= 0;
    }
    public void Expire() 
    {
        //OnExpire();
    }
    
}
