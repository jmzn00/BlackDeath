using System;
using UnityEngine;

/// <summary>
/// A direct damage action. Supports reactive bonus damage.
/// Create via: right-click → Create → Combat/Action/Attack
/// </summary>
[CreateAssetMenu(menuName = "Combat/Action/Attack")]
public class AttackAction : CombatAction
{
    [Header("LEGACY")]
    [Header("Damage")]
    public int damage = 1;
    [Tooltip("Extra damage added when the reactive prompt is hit successfully.")]
    public int bonusDamageOnSuccess = 1;

    public int ApAmount = 1;

    public override bool Resolve(ActionContext context)
    {
        context.Source.AddActionPoints(ApAmount);
        context.Source.PlayAction(context);
        return true;
    }
}
