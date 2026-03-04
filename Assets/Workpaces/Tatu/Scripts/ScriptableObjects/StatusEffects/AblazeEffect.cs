using UnityEngine;

/// <summary>
/// Ablaze: deals fire tick damage at the start of the affected combatant's turn.
/// </summary>
[CreateAssetMenu(menuName = "Combat/StatusEffect/Ablaze")]
public class AblazeEffect : StatusEffect
{
    [Tooltip("Damage dealt at the start of each of the affected combatant's turns.")]
    public int tickDamage = 1;

    private void Reset()
    {
        effectName = "Ablaze";
        duration   = 3;
        tickDamage = 1;
    }

    public override void OnApply(Combatant target)
    {
        Debug.Log($"[Status] {target.name} is ablaze for {duration} turn(s)! ({tickDamage} dmg/turn)");
    }

    public override void OnTurnStart(Combatant target)
    {
        Debug.Log($"[Status] {target.name} burns for {tickDamage} damage.");
        target.ApplyDamage(tickDamage);
    }

    public override void OnExpire(Combatant target)
    {
        Debug.Log($"[Status] {target.name} is no longer ablaze.");
    }
}
