/// <summary>
/// Runtime instance of a StatusEffect on a specific Combatant.
/// Tracks remaining duration separately from the ScriptableObject definition
/// so the same SO asset can be on multiple combatants simultaneously.
/// </summary>
[System.Serializable]
public class ActiveStatusEffect
{
    public StatusEffect effect;
    public int remainingDuration;

    public ActiveStatusEffect(StatusEffect effect)
    {
        this.effect            = effect;
        this.remainingDuration = effect.duration;
    }

    /// <summary>Tick duration down. Returns true if the effect has expired.</summary>
    public bool Tick()
    {
        if (remainingDuration < 0) return false; // permanent
        remainingDuration--;
        return remainingDuration <= 0;
    }
}
