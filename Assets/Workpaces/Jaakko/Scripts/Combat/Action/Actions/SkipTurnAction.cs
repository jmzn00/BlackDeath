using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Action/SkipTurn")]
public class SkipTurnAction : CombatAction
{
    public override bool Resolve(ActionContext context)
    {
        context.Source.PlayAction(context);
        return true;
    }
}
