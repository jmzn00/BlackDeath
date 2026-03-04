using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Animator))]
public class Combatant : MonoBehaviour
{
    // Stats
    [Header("Stats")]
    public int maxHealth = 10;
    public int health = 10;
    public int actionPoints = 0;
    public int maxActionPoints = 3;

    // References
    [Header("References")]
    public Animator animator;
    public AnimationClip idleAnim;

    // Combat
    [Header("Combat")]
    [Tooltip("Actions this combatant can use (AttackAction or SkillAction assets).")]
    public CombatAction[] availableActions;
    [Tooltip("AI behavior — leave null for the player.")]
    public CombatBehavior behavior;

    // Events
    public UnityEvent<int, int> onStatsChanged;
    public UnityEvent<Combatant> onDeath;
    public UnityEvent onStatusEffectsChanged;

    // Runtime state (written by BattleManager)
    [HideInInspector] public CombatAction currentAction;
    [HideInInspector] public Combatant currentTarget;
    [HideInInspector] public int behaviorStateIndex;

    // Active status effects
    private readonly List<ActiveStatusEffect> m_activeEffects = new List<ActiveStatusEffect>();
    public IReadOnlyList<ActiveStatusEffect> ActiveEffects => m_activeEffects;

    private void Reset() => animator = GetComponent<Animator>();

    private void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        onStatsChanged?.Invoke(health, actionPoints);
    }

    // Stat API

    public void ChangeActionPoints(int delta)
    {
        actionPoints = Mathf.Clamp(actionPoints + delta, 0, maxActionPoints);
        onStatsChanged?.Invoke(health, actionPoints);
    }

    public void ApplyDamage(int amount)
    {
        if (amount <= 0) return;
        health = Mathf.Max(0, health - amount);
        onStatsChanged?.Invoke(health, actionPoints);
        if (health <= 0) Die();
    }

    public void Die()
    {
        onDeath?.Invoke(this);
        gameObject.SetActive(false);
    }

    // Status effect API

    public void ApplyStatusEffect(StatusEffect effect)
    {
        if (effect == null) return;
        var existing = m_activeEffects.Find(a => a.effect == effect);
        if (existing != null)
        {
            existing.remainingDuration = effect.duration;
        }
        else
        {
            m_activeEffects.Add(new ActiveStatusEffect(effect));
            effect.OnApply(this);
        }
        onStatusEffectsChanged?.Invoke();
    }

    public void RemoveStatusEffect(StatusEffect effect)
    {
        var active = m_activeEffects.Find(a => a.effect == effect);
        if (active == null) return;
        active.effect.OnExpire(this);
        m_activeEffects.Remove(active);
        onStatusEffectsChanged?.Invoke();
    }

    public bool HasStatusEffect<T>() where T : StatusEffect =>
        m_activeEffects.Exists(a => a.effect is T);

    public bool IsActionPrevented()
    {
        foreach (var a in m_activeEffects)
            if (a.effect.PreventsAction(this)) return true;
        return false;
    }

    public void ProcessTurnStart()
    {
        for (int i = m_activeEffects.Count - 1; i >= 0; i--)
        {
            m_activeEffects[i].effect.OnTurnStart(this);
            if (health <= 0) return;
        }
    }

    public void ProcessTurnEnd()
    {
        bool anyExpired = false;
        for (int i = m_activeEffects.Count - 1; i >= 0; i--)
        {
            var active = m_activeEffects[i];
            active.effect.OnTurnEnd(this);
            if (active.Tick())
            {
                active.effect.OnExpire(this);
                m_activeEffects.RemoveAt(i);
                anyExpired = true;
            }
        }
        if (anyExpired) onStatusEffectsChanged?.Invoke();
    }

    public void PlayAction(CombatAction action, Combatant target)
    {
        currentAction = action;
        currentTarget = target;

        if (action?.animationClip != null)
            animator.Play(action.animationClip.name);
        else
        {
            // No animation: fire damage and resolve immediately
            ApplyHitDamage(dodged: false, parried: false);
            ResolveAction();
        }
    }

    public void OpenReactiveWindow(string promptKey)
    {
        BattleManager.Instance?.OnWindowOpen(this, promptKey);
    }

    public void CloseReactiveWindow()
    {
        BattleManager.Instance?.OnWindowClose(this);
    }

    public void ResolveAction()
    {
        currentAction = null;
        currentTarget = null;
        BattleManager.Instance?.OnActionResolved(this);
    }

    public void ApplyHitDamage(bool dodged, bool parried)
    {
        if (currentAction == null || currentTarget == null) return;

        bool isPlayer = gameObject.CompareTag("Player");

        int damage = 0;

        if (currentAction is AttackAction attack)
        {
            damage = attack.damage;
            // Parry bonus in player attacks
            if (isPlayer && parried) damage += attack.bonusDamageOnSuccess;
            // Enemy attacks, parried or dodged = no damage
            if (!isPlayer && (dodged || parried)) damage = 0;
        }

        // Skills are still TBD, but now similar to attacks
        else if (currentAction is SkillAction skill)
        {
            damage = skill.damage;
            if (isPlayer && parried) damage += skill.bonusDamageOnSuccess;
            if (!isPlayer && (dodged || parried)) damage = 0;
        }

        if (damage > 0) currentTarget.ApplyDamage(damage);

        // STATUS EFFECTS: WORK IN PROGRESS.
        bool applyEffects = isPlayer || (!dodged && !parried);
        if (applyEffects && currentAction is SkillAction s)
        {
            if (s.targetEffects != null)
                foreach (var e in s.targetEffects) currentTarget.ApplyStatusEffect(e);
            if (s.selfEffects != null)
                foreach (var e in s.selfEffects) ApplyStatusEffect(e);
        }

        if (currentAction.grantsApOnUse) ChangeActionPoints(1);
    }

    public CombatAction ChooseAction(BattleManager battleManager)
    {
        if (gameObject.CompareTag("Player")) return null;
        if (availableActions == null || availableActions.Length == 0) return null;
        if (behavior != null) return behavior.ChooseAction(this, battleManager);
        return availableActions[0];
    }
}
