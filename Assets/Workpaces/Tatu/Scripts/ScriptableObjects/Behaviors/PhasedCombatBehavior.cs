using UnityEngine;


[CreateAssetMenu(menuName = "Combat/Behavior/Phased")]
public class PhasedCombatBehavior : CombatBehavior
{
    [System.Serializable] 
    public struct Phase
    {
        [Tooltip("This phase activates when health% is at or BELOW this value. Use 1.0 as a catch-all final phase.")]
        [Range(0f, 1f)] public float healthThreshold;
        public CombatAction[] actions;
    }

    [Tooltip("Order from most-injured to least. First matching phase wins.")]
    public Phase[] phases;

    public override CombatAction ChooseAction(Combatant self, BattleManager manager)
    {
        if (self.availableActions == null || self.availableActions.Length == 0) return null;

        float fraction = self.maxHealth > 0 ? (float)self.health / self.maxHealth : 0f;

        // Walk phases; use first whose threshold >= current fraction (i.e. health has dropped to or below it)
        foreach (var phase in phases)
        {
            if (fraction <= phase.healthThreshold && phase.actions != null && phase.actions.Length > 0)
                return phase.actions[Random.Range(0, phase.actions.Length)];
        }

        return self.availableActions[0];
    }
}
