using UnityEngine;

/// <summary>
/// Stun: the affected combatant skips its next turn.
/// Duration 1 = skip one turn.
/// </summary>
[CreateAssetMenu(menuName = "Combat/StatusEffect/Stun")]
public class StunEffect : StatusEffect
{
    private void Reset()
    {
        effectName = "Stunned";
        duration   = 1;
    }

    public override bool PreventsAction(Combatant target) => true;

    public override void OnApply(Combatant target)
    {
        Debug.Log($"[Status] {target.name} is stunned for {duration} turn(s).");
    }

    public override void OnExpire(Combatant target)
    {
        Debug.Log($"[Status] {target.name} is no longer stunned.");
    }
}
