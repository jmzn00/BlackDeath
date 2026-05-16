using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public enum ActionResult
{
    Hit,
    Dodged,
    Parried,
    Confirmed,
    None
}
public enum ConfirmGrade
{
    Missed,
    Good,
    Perfect
}
public enum TargetType 
{
    Self,
    Ally,
    AOEAlly,
    Enemy,
    AOEEnemy,
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

    [Tooltip("Grant 1 AP to the user on use.")]
    public bool grantsApOnUse = false;

    public float baseDamage = 1;
    public float confirmDamageMultipler = 1.2f;
    public float confirmPerfectMultiplier = 1.5f;
    [Tooltip("Fraction (0-1) of the window that must have elapsed before the press counts as Perfect. E.g. 0.65 = press in the last 35% of the window.")]
    [Range(0f, 1f)] public float confirmPerfectFraction = 0.65f;

    public List<ActorStatusEffect> AppliedEffects = new List<ActorStatusEffect>();

    public TargetType targetType = TargetType.Enemy;

    [Tooltip("If false, it will not open or close the window and will be treated as confirm")]
    public bool isReactive = true;

    [Header("Audio")]
    public AudioClip transitionSound;  // actor walks toward target
    public AudioClip attackSound;      // attack/skill animation starts
    public AudioClip strikeSound;      // Anim_OpenWindow hit frame
    public bool useHumanFootsteps;     // play footstep bank during transition
    /// <summary>
    /// Execute this action. Called by Combatant.PlayAction.
    /// Base implementation plays the animation; override for special logic.
    /// </summary>
    
    public List<CombatActor> GetValidTargets(CombatActor source,
        List<CombatActor> participants)
    {
        List<CombatActor> validTargets = new List<CombatActor>(participants);
        switch (targetType) 
        {
            case TargetType.Enemy:
            case TargetType.AOEEnemy:
                return validTargets.Where(p => p.Team != source.Team && !p.IsDead).ToList();                
            case TargetType.Ally:
            case TargetType.AOEAlly:
                return validTargets.Where(p => p.Team == source.Team && !p.IsDead).ToList();
            case TargetType.Self:
                return validTargets.Where(p => p == source).ToList();
            case TargetType.Any:
                return validTargets;
        }
        return validTargets;
    }
    public virtual bool CanExecute(CombatActor source, out string reason) 
    {
        bool blocked = false;
        reason = "";

        foreach (var i in source.CurrentStatusEffects) 
        {            
            if (!i.Template.CanPerformAction(this, out string r)) 
            {
                reason += r + "\n";
                blocked = true;
            }
        }
        int sourceAP = source.ActionPoints;
        if (sourceAP - apCost < 0) 
        {
            reason = "Not Enough Action Points";
            blocked = true;
        }
        return !blocked;
    }
    public abstract bool Resolve(ActionContext context);
}
