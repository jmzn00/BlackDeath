using UnityEngine;

/// <summary>
/// A skill action that deals optional damage and applies status effects to the target.
/// Create via: right-click → Create → Combat/Action/Skill
/// </summary>
[CreateAssetMenu(menuName = "Combat/Action/Skill")]
public class SkillAction : CombatAction
{
    [Header("Damage (optional)")]
    [Tooltip("Set to 0 for a pure utility/debuff skill.")]
    public int damage = 0;
    public int bonusDamageOnSuccess = 0;

    [Header("Status Effects")]
    [Tooltip("Effects applied to the TARGET on hit.")]
    public StatusEffect[] targetEffects;

    [Tooltip("Effects applied to the USER (self-buffs) on use.")]
    public StatusEffect[] selfEffects;

    public override void Resolve(CombatContext context)
    {
        throw new System.NotImplementedException();
    }
}
