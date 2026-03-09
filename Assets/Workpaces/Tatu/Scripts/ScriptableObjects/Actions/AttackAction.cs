using System;
using UnityEngine;

/// <summary>
/// A direct damage action. Supports reactive bonus damage.
/// Create via: right-click → Create → Combat/Action/Attack
/// </summary>
[CreateAssetMenu(menuName = "Combat/Action/Attack")]
public class AttackAction : CombatAction
{
    [Header("Damage")]
    public int damage = 1;
    [Tooltip("Extra damage added when the reactive prompt is hit successfully.")]
    public int bonusDamageOnSuccess = 1;

    private float additionalDamage = 0f;

    public override bool Resolve(ActionContext context, Action OnComplete)
    {
        context.Source.PlayAction(context, OnComplete);
        return true;
    }
    protected override void OnHit(ActionContext ctx)
    {
        //Debug.Log($"Attack from {ctx.Source.name} Hit");
        ctx.Target.Health.ApplyDamage(damage + additionalDamage);
        foreach(var e in AppliedEffects) 
        {
            ctx.Target.ApplyStatus(e);
        }

        base.OnHit(ctx);
    }
    protected override void OnDodged(ActionContext ctx)
    {
        //Debug.Log($"Attack from {ctx.Source.name} Dodged");
    }    
    protected override void OnParried(ActionContext ctx)
    {
        //Debug.Log($"Attack from {ctx.Source.name} Parried");
        ctx.Source.Health.ApplyDamage(damage);
        base.OnParried(ctx);
    }
    public override void OnConfirmed(ActionContext ctx)
    {
        //Debug.Log("Attack Confirmed");

        additionalDamage = bonusDamageOnSuccess;
        base.OnConfirmed(ctx);
    }
}
