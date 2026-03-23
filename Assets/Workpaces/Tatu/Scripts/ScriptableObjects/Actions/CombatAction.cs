using System;
using System.Collections.Generic;
using UnityEngine;
public enum ActionResult 
{
    Hit,
    Dodged,
    Parried,
    Confirmed,
    None
}
public enum TargetType 
{
    Self,
    Ally,
    Enemy,
    Any
}

/// <summary>
/// Base class for all combat actions. Use AttackAction or SkillAction subclasses.
/// Shared fields: name, AP cost, animation, reactive flag.
/// </summary>
public abstract class CombatAction : ScriptableObject
{
    [Header("Info")]
    public string actionName = "Action";
    [TextArea] public string description;

    [Header("Cost")]
    public int apCost = 0;

    [Header("Animation")]
    public AnimationClip animationClip;

    [Header("Reactive")]
    [Tooltip("If true, the animation should fire StartReactiveWindow events.")]
    public bool isReactive = true;
    [Tooltip("Grant 1 AP to the user on use.")]
    public bool grantsApOnUse = false;

    public float baseDamage = 1;
    public float confirmDamageMultipler = 1.2f;

    public List<ActorStatusEffect> AppliedEffects = new List<ActorStatusEffect>();

    public TargetType targetType;
    /// <summary>
    /// Execute this action. Called by Combatant.PlayAction.
    /// Base implementation plays the animation; override for special logic.
    /// </summary>
    
    public static List<CombatActor> GetValidTargets(CombatActor source,
        List<CombatActor> participants,
        TargetType type)
    {
        switch (type) 
        {
            case TargetType.Enemy:

                break;
            case TargetType.Ally:

                break;
            case TargetType.Self:

                break;
            case TargetType.Any:

                break;
        }
        return null;
    }
    public virtual bool CanExecute(CombatActor source, CombatActor target, out string reason) 
    {
        bool blocked = false;
        reason = "";
        // check ap 

        if (!IsValidTarget(this, source, target)) 
        {
            reason = "Invalid Target";
            return false;
        }

        foreach (var e in source.StatusEffects) 
        {            
            if (!e.CanPerformAction(this, target, out string r)) 
            {
                reason += r + "\n";
                blocked = true;
            }
        }        
        return !blocked;
    }
    public virtual bool IsValidTarget(CombatAction action, CombatActor source, CombatActor target) 
    {
        return true;
    }
    public abstract bool Resolve(ActionContext context, Action OnComplete);

    public virtual void ResolveResult(ActionContext ctx,  ActionResult result) 
    {
        switch (result) 
        {
            case ActionResult.Hit:
                OnHit(ctx);
                break;
            case ActionResult.Dodged:
                OnDodged(ctx);
                break;
            case ActionResult.Parried:
                OnParried(ctx);
                break;
            case ActionResult.Confirmed:
                OnConfirmed(ctx);
                break;
            case ActionResult.None:
                OnNone(ctx);
                break;
        }
    } 
    protected virtual void OnHit(ActionContext ctx) 
    {
        ctx.Target.Health.ApplyDamage(baseDamage);          
    }
    protected virtual void OnDodged(ActionContext ctx) 
    {
        
    }
    protected virtual void OnParried(ActionContext ctx) 
    {
        ctx.Source.Health.ApplyDamage(baseDamage);
    }
    protected virtual void OnConfirmed(ActionContext ctx) 
    {
        ctx.Target.Health.ApplyDamage(baseDamage * confirmDamageMultipler);

        foreach (var e in AppliedEffects)
        {
            ctx.Target.ApplyStatus(e);
        }
    }
    protected virtual void OnNone(ActionContext ctx) 
    {
    
    }
}
