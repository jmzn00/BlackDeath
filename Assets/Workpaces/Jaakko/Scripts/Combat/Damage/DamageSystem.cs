using UnityEngine;
using System.Collections.Generic;


public class DamageSystem
{
    public DamageSystem() 
    {
        CombatEvents.OnTurnStarted += ActorTurnStart;
        CombatEvents.OnTurnEnded += ActorTurnEnd;
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
                Debug.Log($"{target.name} recieved effect {e.displayName}");
            }
        }
    }
    public void ApplyDamage(float amount, IDamageSource source, CombatActor target) 
    {
        Debug.Log($"{source.SourceName} applied {amount} damage to {target.name}");

        target.Health.ApplyDamage(amount);
    }
    public void ApplyHeal(float amount, IDamageSource source, CombatActor target) 
    {
        Debug.Log($"{source.SourceName} applied {amount} healing to {target.name}");
        target.Health.ApplyHealth(amount);
    }
    public void ActorTurnStart(CombatActor actor) 
    {        
        foreach (var i in actor.CurrentStatusEffects) 
        {
            i.TurnStart();
        }
    }
    public void ActorTurnEnd(CombatActor actor) 
    {
        foreach (var i in actor.CurrentStatusEffects) 
        {
            i.TurnEnd();
        }
    }
}
