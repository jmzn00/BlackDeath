using UnityEngine;
using System.Collections.Generic;

public class DamageSystem : CombatSystemBase
{
    private ActionSystem m_action;
    public DamageSystem(ActionSystem action) 
    {
        m_action = action;        
    }
    public override void Init(CombatContext context)
    {
        m_action.OnActionResolved += ActionResolved;

        CombatEvents.OnTurnStarted += ActorTurnStart;
        CombatEvents.OnTurnEnded += ActorTurnEnd;
    }
    public override void Reset()
    {
        m_action.OnActionResolved -= ActionResolved;

        CombatEvents.OnTurnStarted -= ActorTurnStart;
        CombatEvents.OnTurnEnded -= ActorTurnEnd;
    }
    public void ActionResolved(ActionContext ctx, ActionResult result) 
    {
        if (ctx.Targets == null || ctx.Targets.Count == 0) 
        {
            Debug.LogWarning($"No Targets on {ctx.Action.actionName}");
            return;
        }
        Debug.Log($"{ctx.Action.actionName} result {result}  ");
        CombatAction a = ctx.Action;
        switch (result) 
        {
            case ActionResult.Confirmed:
                float multiplier = ctx.ConfirmGrade == ConfirmGrade.Perfect
                    ? a.confirmPerfectMultiplier
                    : a.confirmDamageMultipler;
                foreach (var t in ctx.Targets)
                {
                    ApplyDamage(a.baseDamage * multiplier, ctx.Source, t);
                    UpdateStatusEffects(a.AppliedEffects, t, ctx.Source);
                }
                break;
            case ActionResult.Hit:
                foreach (var t in ctx.Targets)
                {
                    ApplyDamage(a.baseDamage, ctx.Source, t);
                }
                break;
            case ActionResult.Parried:
                ApplyDamage(a.baseDamage, ctx.PrimaryTarget, ctx.Source);
                break;
            case ActionResult.Dodged:
                
                break;
        }        
    }
    private void UpdateStatusEffects(List<ActorStatusEffect> effects, CombatActor target, CombatActor source) 
    {
        if (target == null) return;

        foreach (var e in effects)
        {
            if (target.HasEffect(e, out StatusEffectInstance i))
            {
                if (!e.isStackable) return;
                
                i.UpdateDuration(e.duration);
                Debug.Log($"{target.name} {e.displayName} duration increased to {i.RemainingTurns}");
            }
            else
            {
                Debug.Log($"{target.name} recieved {e.displayName}");
                target.ApplyEffect(new StatusEffectInstance(e, target, source, this));
            }
        }
    }
    public void ApplyDamage(float amount, IDamageSource source, CombatActor target) 
    {
        if (source.SourceActor.IsDead) 
        {
            Debug.Log($"Cannot apply damage from {source.SourceName}, they are dead");
            return;
        }

        if (amount > 0f) 
        {
            target.Health.ApplyDamage(amount);
            CombatEvents.DamageApplied(target, source, amount);
        }
                
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
    private static bool IsDead(CombatActor actor)
    {
        return !(actor.Health.CurrentHealth > 0f);
    }
    private static void ActorTurnStart(CombatActor actor)
    {
        var effects = new List<StatusEffectInstance>(actor.CurrentStatusEffects);

        for (int i = effects.Count - 1; i >= 0; i--)
        {
            var instance = effects[i];
            instance.TurnStart();
        }
    }

    private void ActorTurnEnd(CombatActor actor) 
    {
        var effects = new List<StatusEffectInstance>(actor.CurrentStatusEffects);

        for (int i = effects.Count - 1; i >= 0; i--) 
        {
            var instance = effects[i];
            instance.TurnEnd();
        }        
    }
}
