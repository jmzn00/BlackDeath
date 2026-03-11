using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Action/SkipTurn")]
public class SkipTurnAction : CombatAction
{
    public override bool Resolve(ActionContext context, Action OnComplete)
    {
        OnComplete?.Invoke();

        return true;
    }
}
