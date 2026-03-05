using UnityEngine;

/// <summary>
/// A direct damage action. Supports reactive bonus damage.
/// Create via: right-click → Create → Combat/Action/Attack
/// </summary>
[CreateAssetMenu(menuName = "Combat/Action/Attack")]
public class AttackAction : CombatAction
{
    [Header("Damage")]
    public int damage = 1;
    [Tooltip("Extra damage added when the reactive prompt is hit successfully.")]
    public int bonusDamageOnSuccess = 1;

    public override void Resolve(CombatContext context)
    {
        throw new System.NotImplementedException();
    }
}
