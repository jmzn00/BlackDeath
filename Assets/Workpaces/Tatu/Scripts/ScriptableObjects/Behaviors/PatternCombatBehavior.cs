using UnityEngine;

/// <summary>
/// Repeats a fixed pattern of actions (by index into availableActions).
/// If pattern is empty, automatically cycles through all available actions.
/// </summary>
[CreateAssetMenu(menuName = "Combat/Behavior/Pattern")]
public class PatternCombatBehavior : CombatBehavior
{
    [Tooltip("Indices into the combatant's availableActions array, cycled each turn. Leave empty to auto-cycle through all actions.")]
    public int[] pattern;

    public override CombatAction ChooseAction(Combatant self, BattleManager manager)
    {
        if (self.availableActions == null || self.availableActions.Length == 0) return null;

        int actionIndex;

        // If no pattern is defined, auto-generate one that cycles through all actions
        if (pattern == null || pattern.Length == 0)
        {
            actionIndex = self.behaviorStateIndex % self.availableActions.Length;
            self.behaviorStateIndex++;
        }
        else
        {
            // Use the defined pattern
            int state = Mathf.Clamp(self.behaviorStateIndex, 0, pattern.Length - 1);
            actionIndex = Mathf.Clamp(pattern[state], 0, self.availableActions.Length - 1);
            self.behaviorStateIndex = (state + 1) % pattern.Length;
        }

        return self.availableActions[actionIndex];
    }
}
