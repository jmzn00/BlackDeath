using UnityEngine.InputSystem;
using UnityEngine;

public class PlayerCombatActor : CombatActor
{
    private UIManager m_ui;
    private InputManager m_input;
    protected override void OnInitliazed(GameManager game)
    {
        m_ui = game.Resolve<UIManager>();
        m_input = game.Resolve<InputManager>();

        m_input.InputActions.Combat.Parry.performed += HandleParry;
        m_input.InputActions.Combat.Dodge.performed += HandleDodge;

        SetActionProvider(new PlayerActionProvider(m_combatManager, m_ui.Controller));
        SetReactionProvider(new PlayerReactionProvider(this));

        OnTargeted += OnActorTargeted;
        OnNoLongerTargeted += OnActorNoLongerTargeted;

    }
    protected override void OnDispose()
    {
        m_input.InputActions.Combat.Parry.performed -= HandleParry;
        m_input.InputActions.Combat.Dodge.performed -= HandleDodge;

        OnTargeted -= OnActorTargeted;
        OnNoLongerTargeted -= OnActorNoLongerTargeted;
    }
    private void OnActorTargeted(CombatActor source, CombatActor target, CombatAction action) 
    {
        if (target == this && source != this) 
        {
            m_currentlyTargeted = true;
        }
    }
    private void OnActorNoLongerTargeted(CombatActor source, CombatActor target, CombatAction action) 
    {
        if (target == this && source != this) 
        {
            m_currentlyTargeted = false;
        }
    }

    private void HandleParry(InputAction.CallbackContext ctx) 
    {
        if (m_combatManager.State == CombatState.None
            && !Actor.IsControlled) return;
        if (m_defensiveAnimationPlaying) return;
        if (!m_currentlyTargeted) return;

        OnParryPerformed();
    }
    private void HandleDodge(InputAction.CallbackContext ctx) 
    {
        if (m_combatManager.State == CombatState.None
            && !Actor.IsControlled) return;
        if (m_defensiveAnimationPlaying) return;
        if (!m_currentlyTargeted) return;

        OnDodgePerformed();
    }
    public override void OnCombatFinished()
    {
        base.OnCombatFinished();

        if (IsDead)
            IsDead = false;

        if (m_visual)
            m_visual.SetActive(true);

        Health.ApplyHealth(Health.MaxHealth);
    }
}
