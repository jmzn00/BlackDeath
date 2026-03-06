using UnityEngine;

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

    /// <summary>
    /// Execute this action. Called by Combatant.PlayAction.
    /// Base implementation plays the animation; override for special logic.
    /// </summary>
    public virtual void Execute(Combatant executor, Combatant target)
    {
        executor.PlayAction(this, target);
    }
    public abstract bool Resolve(CombatContext context);
}
