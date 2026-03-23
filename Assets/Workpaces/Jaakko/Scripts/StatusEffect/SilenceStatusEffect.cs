using UnityEngine;

[CreateAssetMenu(menuName = "Actor/StatusEffects/SilenceEffect")]
public class SilenceStatusEffect : ActorStatusEffect
{
    public override bool CanPerformAction(CombatAction action, out string reason)
    {
        reason = "";
        if (action is SkillAction) 
        {
            reason = "Silenced";
            return false;
        }
        return true;
        
    }
}
