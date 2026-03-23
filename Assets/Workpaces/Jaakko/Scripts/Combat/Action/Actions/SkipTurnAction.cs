using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Action/SkipTurn")]
public class SkipTurnAction : CombatAction
{
    public override bool IsValidTarget(CombatAction action, CombatActor source, CombatActor target)
    {
        return target.IsPlayer;
    }
    public override bool Resolve(ActionContext context, Action OnComplete)
    {
        context.Target = context.Source;

        context.Source.PlayAction(context, OnComplete);
        return true;
    }
}
