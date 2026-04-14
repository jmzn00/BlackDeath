using UnityEngine;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;
using System.Linq;


public class DamageSystem : CombatSystemBase
{
    private CombatContext m_context;
    private ActionSystem m_action;
    public DamageSystem(ActionSystem action) 
    {
        m_action = action;        
    }
    public override void Init(CombatContext context)
    {
        m_context = context;

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
        float finalDamage = 0f;
        CombatActor reciever = ctx.Target;

        switch (result) 
        {
            case ActionResult.Confirmed:
                finalDamage = ctx.Action.baseDamage
                    * ctx.Action.confirmDamageMultipler;                
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
        List<CombatActor> targets = GetTargets(ctx);
        foreach (CombatActor target in targets) 
        {
            ApplyDamage(finalDamage, ctx.Source, target);
            if (result == ActionResult.Confirmed) 
            {
                UpdateStatusEffects(ctx.Action.AppliedEffects, target, ctx.Source);
            }
        }        
    }
    private List<CombatActor> GetTargets(ActionContext actx) 
    {
        List<CombatActor> targets = new();
        switch (actx.Action.targetType) 
        {
            case TargetType.Enemy:
                targets.Add(actx.Target);
                break;
            case TargetType.AOEAlly:
            case TargetType.AOEEnemy:
                targets = actx.Action.GetValidTargets(actx.Source, m_context.Actors.ToList());
                break;
            case TargetType.Self:
                targets.Add(actx.Source);
                break;
            case TargetType.Ally:
                targets.Add(actx.Target);
                break;
            case TargetType.Any:
                targets.Add(actx.Target);
                break;
            default:
                Debug.LogWarning($"Unhandled target type {actx.Action.targetType}");
                break;
        }
        return targets;
    }
    private void UpdateStatusEffects(List<ActorStatusEffect> effects, CombatActor target, CombatActor source) 
    {
        if (target == null) return;

        foreach (var e in effects)
        {
            if (target.HasEffect(e))
            {
                if (!e.isStackable) return;

                StatusEffectInstance i = target.GetInstance(e);
                if (i != null)
                {
                    i.UpdateDuration(e.duration);
                }
                else
                {
                    Debug.LogWarning($"Couldnt find instance for {e.displayName}");
                }
            }
            else
            {
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

        target.Health.ApplyDamage(amount);
        
        if (IsDead(target)) 
        {
            target.SetDead(true);
            CombatEvents.ActorDied(target);
        }

        if (amount > 0f)
            CombatEvents.DamageApplied(target, source, amount);
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
        List<StatusEffectInstance> effects = new List<StatusEffectInstance>(actor.CurrentStatusEffects);

        for (int i = effects.Count - 1; i >= 0; i--)
        {
            StatusEffectInstance instance = effects[i];
            instance.TurnStart();
        }
    }

    public void ActorTurnEnd(CombatActor actor) 
    {
        List<StatusEffectInstance> effects = new List<StatusEffectInstance>(actor.CurrentStatusEffects);

        for (int i = effects.Count - 1; i >= 0; i--) 
        {
            StatusEffectInstance instance = effects[i];
            instance.TurnEnd();
        }        
    }
}
