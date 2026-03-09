using System.Collections.Generic;
using UnityEngine;

public enum ReactionType 
{
    None,
    Parry,
    Dodge,
    Confirm
}
public class ReactiveWindow
{
    private bool m_windowOpen = false;

    private float m_parryWindow = 0.4f;
    private float m_dodgeWindow = 0.7f;

    private float m_parryCooldownTimer;
    private float m_dodgeCooldownTimer;
    private float m_confirmCooldownTimer;   

    private float m_parryCooldown = 0.1f;
    private float m_dodgeCooldown = 0.1f;
    private float m_confirmCooldown = 0.1f;


    private float m_time;
    private float m_confirmTime;

    private bool m_confirmed;
    public bool CanConfirm => m_windowOpen
        && !m_confirmed
        && m_confirmTime <= m_confirmCooldownTimer;

    public bool IsOpen => m_windowOpen;
    public float Time => m_time;
    public bool CanParry => m_windowOpen
        && m_time <= m_parryWindow;
    public bool CanDodge => m_windowOpen && m_time <= m_dodgeWindow;

    private InputPromptLibrary m_promptLibrary;
    private List<InputPrompt> m_defenderPrompts = new();
    public List<InputPrompt> DefenderPrompts => m_defenderPrompts;

    private CombatActor m_reactor;
    public bool IsPlayerReactor => m_reactor != null && m_reactor.IsPlayer;
    

    private ReactionType m_attackerReaction;
    private ReactionType m_defenderReaction;

    public ReactiveWindow(InputPromptLibrary promptLibrary) 
    {
        m_promptLibrary = promptLibrary;
        SetDefenderPrompts();
    }
    public void SetDefenderPrompts() 
    {
        m_defenderPrompts.Clear();

        AddPromptIfValid("DodgePrompt");
        AddPromptIfValid("ParryPrompt");
    }
    private void AddPromptIfValid(string key)
    {
        var prompt = m_promptLibrary.Get(key);
        if (prompt != null)
        {
            prompt.action.Enable();
            m_defenderPrompts.Add(prompt);
        }
        else 
        {
            Debug.LogWarning("Couldnt get Prompt");
            return;
        }
    }
    public void Open(ActionContext ctx) 
    {
        m_windowOpen = true;
        m_reactor = ctx.Target;
        
        m_attackerReaction = ReactionType.None;
        m_defenderReaction = ReactionType.None;

        foreach (var prompt in m_defenderPrompts) 
        {
            prompt.action.Enable();
        }
    }

    public void TryActivateParry() 
    {
        if (!CanParry) return;

        if (m_parryCooldownTimer > 0f) return;
        if (m_defenderReaction != ReactionType.None) return;

        m_defenderReaction = ReactionType.Parry;
        m_parryCooldownTimer = m_parryCooldown;
    }
    public void TryActivateDodge() 
    {
        if (!CanDodge) return;

        if (m_dodgeCooldownTimer > 0f) return;
        if (m_defenderReaction != ReactionType.None) return;

        m_defenderReaction = ReactionType.Dodge;
        m_dodgeCooldownTimer = m_dodgeCooldown;
    }
    public void TryConfirmAttacker()
    {
        if (!CanConfirm || m_attackerReaction != ReactionType.None) return;
        m_attackerReaction = ReactionType.Confirm;

        m_confirmCooldownTimer = m_confirmCooldown;
    }

    public ReactionType ConsumeDefenderReaction() 
    {
        ReactionType result = m_defenderReaction;
        m_defenderReaction = ReactionType.None;
        m_time = 0f;        
        return result;
    }
    public ReactionType ConsumeAttackerReaction()
    {
        var result = m_attackerReaction;
        m_attackerReaction = ReactionType.None;
        m_confirmTime = 0f;
        return result;
    }

    // combat manager calls reset when the window closes
    public void Reset() 
    {
        m_windowOpen = false;
        m_confirmed = false;

        m_defenderReaction = ReactionType.None;
        m_attackerReaction = ReactionType.None;

        m_parryCooldownTimer = 0f;
        m_dodgeCooldownTimer = 0f;
        m_time = 0f;
    }
    public void Update(float dt)
    {
        if (m_parryCooldownTimer > 0f)
            m_parryCooldownTimer -= dt;
        if (m_dodgeCooldownTimer > 0f)
            m_dodgeCooldownTimer -= dt;
        if (m_confirmCooldownTimer > 0f)
            m_confirmCooldownTimer -= dt;

        if (!m_windowOpen) return;
        m_time += dt;
        m_confirmTime += dt;
    }
}
