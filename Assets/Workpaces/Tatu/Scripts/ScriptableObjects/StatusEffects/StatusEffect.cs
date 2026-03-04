using UnityEngine;

/// <summary>
/// Base class for all status effects. Subclass this to create new effects.
///
/// Lifecycle per turn:
///   OnApply()     — called once when the effect is first added to a combatant
///   OnTurnStart() — called at the start of every turn belonging to the affected combatant
///   OnTurnEnd()   — called at the end of that combatant's turn
///   OnExpire()    — called once when duration hits 0 and the effect is removed
///
/// To create a new effect: right-click → Create → Combat/StatusEffect/YourEffect
/// </summary>
public abstract class StatusEffect : ScriptableObject
{
    [Header("Info")]
    public string effectName = "Effect";
    [TextArea] public string description;
    public Sprite icon;

    [Header("Duration")]
    [Tooltip("How many of the affected combatant's turns this lasts. -1 = permanent until dispelled.")]
    public int duration = 1;

    // ── Override these in subclasses ─────────────────────────────────────

    /// <summary>Called once when this effect is applied.</summary>
    public virtual void OnApply(Combatant target) { }

    /// <summary>Called at the start of the affected combatant's turn.</summary>
    public virtual void OnTurnStart(Combatant target) { }

    /// <summary>Called at the end of the affected combatant's turn.</summary>
    public virtual void OnTurnEnd(Combatant target) { }

    /// <summary>Called when the effect expires (duration reaches 0).</summary>
    public virtual void OnExpire(Combatant target) { }

    /// <summary>
    /// Return true if this effect should block the combatant from acting this turn.
    /// Default: false. Override in e.g. StunEffect.
    /// </summary>
    public virtual bool PreventsAction(Combatant target) => false;
}
