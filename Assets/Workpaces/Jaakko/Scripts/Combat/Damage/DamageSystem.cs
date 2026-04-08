using UnityEngine;
using System.Collections.Generic;


public class DamageSystem : CombatSystemBase
{
    public DamageSystem() 
    {
        
    }
    public override void Init()
    {
        CombatEvents.OnTurnStarted += ActorTurnStart;
        CombatEvents.OnTurnEnded += ActorTurnEnd;
    }
    public override void Dispose()
    {
        CombatEvents.OnTurnStarted -= ActorTurnStart;
        CombatEvents.OnTurnEnded -= ActorTurnEnd;
    }
    public void ActionResolved(ActionContext ctx, ActionResult result) 
    {
        float finalDamage = 0f;
        CombatActor reciever = ctx.Target;

        switch (result) 
        {
            case ActionResult.Confirmed:
                finalDamage = ctx.Action.baseDamage
                    * ctx.Action.confirmDamageMultipler;
                UpdateStatusEffects(ctx.Action.AppliedEffects, ctx.Target);
                break;
            case ActionResult.Hit:
                finalDamage = ctx.Action.baseDamage;
                break;
            case ActionResult.Parried:
                finalDamage = ctx.Action.baseDamage;
                reciever = ctx.Source;
                break;
            case ActionResult.Dodged:
                finalDamage = 0f;
                break;
        }
        ApplyDamage(finalDamage, ctx.Source, reciever);
    }
    private void UpdateStatusEffects(List<ActorStatusEffect> effects, CombatActor target) 
    {
        foreach (var e in effects)
        {
            if (target.HasEffect(e))
            {
                if (!e.isStackable) return;

                StatusEffectInstance i =target.GetInstance(e);
                if (i != null)
                {
                    i.UpdateDuration(e.duration);
                    Debug.Log($"{e.displayName} on {target.name} duration updated to {i.RemainingTurns}");
                }
                else
                {
                    Debug.LogWarning($"Couldnt find instance for {e.displayName}");
                }
            }
            else
            {
                target.ApplyEffect(new StatusEffectInstance(e, target, this));
            }
        }
    }
    public void ApplyDamage(float amount, IDamageSource source, CombatActor target) 
    {
        if (amount > 0f)
            CombatEvents.DamageApplied(target, source, amount);

        target.Health.ApplyDamage(amount);
        
        if (IsDead(target)) 
        {
            target.SetDead(true);
            CombatEvents.ActorDied(target);
        }
    }
    public void ApplyHeal(float amount, IDamageSource source, CombatActor target, CombatActor sourceActor = null) 
    {       
        if (amount > 0f)
            CombatEvents.HealApplied(target, source, amount);

        target.Health.ApplyHealth(amount);
    }
    private bool IsDead(CombatActor actor) 
    {
        if (actor.Health.CurrentHealth <= 0f) 
        {
            return true;
        }
        return false;
    }
    public void ActorTurnStart(CombatActor actor)
    {
        for (int i = actor.CurrentStatusEffects.Count - 1; i >= 0; i--)
        {
            StatusEffectInstance instance = actor.CurrentStatusEffects[i];
            instance.TurnStart();
        }
    }
    public void ActorTurnEnd(CombatActor actor) 
    {
        for (int i = actor.CurrentStatusEffects.Count - 1; i >= 0; i--) 
        {
            StatusEffectInstance instance = actor.CurrentStatusEffects[i];
            instance.TurnEnd();
        }        
    }
}
