using Mono.Cecil;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
public enum ActionResult 
{
    Hit,
    Dodged,
    Parried
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

    public List<ActorStatusEffect> AppliedEffects = new List<ActorStatusEffect>();

    /// <summary>
    /// Execute this action. Called by Combatant.PlayAction.
    /// Base implementation plays the animation; override for special logic.
    /// </summary>
    public virtual void Execute(Combatant executor, Combatant target)
    {
        executor.PlayAction(this, target);
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
        }
    } 
    protected virtual void OnHit(ActionContext ctx) { }
    protected virtual void OnDodged(ActionContext ctx) { }
    protected virtual void OnParried(ActionContext ctx) { }
    public virtual void OnConfirmed(ActionContext ctx) { }
}
