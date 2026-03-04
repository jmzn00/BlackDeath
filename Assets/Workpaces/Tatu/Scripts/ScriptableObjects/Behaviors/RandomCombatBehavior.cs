using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Behavior/Random")]
public class RandomCombatBehavior : CombatBehavior
{
    public override CombatAction ChooseAction(Combatant self, BattleManager manager)
    {
        if (self.availableActions == null || self.availableActions.Length == 0) return null;
        return self.availableActions[Random.Range(0, self.availableActions.Length)];
    }
}
