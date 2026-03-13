using UnityEngine.InputSystem;
using UnityEngine;

public class PlayerCombatActor : CombatActor
{
    private InputManager m_input;
    protected override void OnInitliazed(GameManager game)
    {
        m_input = game.Resolve<InputManager>();

        m_input.InputActions.Combat.Parry.performed += HandleParry;
        m_input.InputActions.Combat.Dodge.performed += HandleDodge;

        SetActionProvider(new PlayerActionProvider());
        SetReactionProvider(new PlayerReactionProvider(this));
    }
    protected override void OnDispose()
    {
        m_input.InputActions.Combat.Parry.performed -= HandleParry;
        m_input.InputActions.Combat.Dodge.performed -= HandleDodge;
    }
    private void HandleParry(InputAction.CallbackContext ctx) 
    {
        /*
        if (m_combatManager.State == CombatState.None
            && !Actor.IsControlled) return;
        if (m_defensiveAnimationPlaying) return;
        if (!m_currentlyTargeted) return;

        OnParryPerformed();
        */
    }
    private void HandleDodge(InputAction.CallbackContext ctx) 
    {
        /*
        if (m_combatManager.State == CombatState.None
            && !Actor.IsControlled) return;
        if (m_defensiveAnimationPlaying) return;
        if (!m_currentlyTargeted) return;

        OnDodgePerformed();
        */
    }
    protected override void CombatEnded(CombatResult result)
    {
        base.CombatEnded(result);
        Debug.Log($"{name} Combat Ended");
        if (IsDead)
            IsDead = false;

        if (m_visual) 
        {
            m_visual.SetActive(true);
            Debug.Log($"VISUAL SET ACTIVE {name}");
        }
        else 
        {
            Debug.Log($"NO VISUAL ON {name}");
        }


            Health.ApplyHealth(Health.MaxHealth);
    }
}
