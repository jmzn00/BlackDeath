using UnityEngine;

/// <summary>
/// Repeats a fixed pattern of actions (by index into availableActions).
/// </summary>
[CreateAssetMenu(menuName = "Combat/Behavior/Pattern")]
public class PatternCombatBehavior : CombatBehavior
{
    [Tooltip("Indices into the combatant's availableActions array, cycled each turn.")]
    public int[] pattern;

    public override CombatAction ChooseAction(Combatant self, BattleManager manager)
    {
        if (pattern == null || pattern.Length == 0) return null;
        if (self.availableActions == null || self.availableActions.Length == 0) return null;

        int state = Mathf.Clamp(self.behaviorStateIndex, 0, pattern.Length - 1);
        int actionIndex = Mathf.Clamp(pattern[state], 0, self.availableActions.Length - 1);
        self.behaviorStateIndex = (state + 1) % pattern.Length;
        return self.availableActions[actionIndex];
    }
}
