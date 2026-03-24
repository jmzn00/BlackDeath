using UnityEngine;
[CreateAssetMenu(menuName = "Actor/StatusEffects/StunStatusEffect")]
public class StunStatusEffect : ActorStatusEffect
{
    public override void OnTurnEnd(StatusEffectInstance instance)
    {
        instance.TickDuration();

        base.OnTurnEnd(instance);
    }
    public override bool CanPerformAction(CombatAction action, out string reason)
    {
        reason = string.Empty;
        if (action is AttackAction || action is SkillAction) 
        {
            reason = "Stunned";
            return false;
        }
        return true;
    }
}
