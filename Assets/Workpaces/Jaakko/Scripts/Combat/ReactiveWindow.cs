using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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

    public readonly float m_parryWindow = 0.3f;
    public readonly float m_dodgeWindow = 0.5f;
    public readonly float m_confirmWindow = 0.5f;

    private float m_parryCooldownTimer;
    private float m_dodgeCooldownTimer;
    private float m_confirmCooldownTimer;   

    private float m_parryCooldown = 0.1f;
    private float m_dodgeCooldown = 0.1f;
    private float m_confirmCooldown = 0.1f;


    private float m_attackerTime;
    private float m_defenderTime;
  
    private ReactionType m_attackerReaction;
    private ReactionType m_defenderReaction;

    public event Action<bool> OnParryWindowOpened;
    public event Action<bool> OnDodgeWindowOpened;
    public event Action<bool> OnConfirmWindowOpened;

    public event Action<ActionContext> OnWindowClosed;

    private bool CanParry => (m_defenderTime <= m_parryWindow)
        && m_parryCooldownTimer <= 0f;
    private bool CanDodge => (m_defenderTime <= m_dodgeWindow)
        && m_dodgeCooldownTimer <= 0f;
    private bool CanConfirm => m_attackerTime <= m_confirmWindow
        && m_confirmCooldownTimer <= 0f;

    private ActionContext m_context;
    private List<InputPrompt> m_defenderPrompts = new();
    private InputPromptLibrary m_promptLibrary;
    private InputPromptLibrary PromptLibrary
    {
        get
        {
            if (m_promptLibrary == null)
            {
                m_promptLibrary =
                    Resources.Load<InputPromptLibrary>("InputPrompts/PromptLibrary");
                if (m_promptLibrary == null)
                    Debug.LogError("InputPromptLibrary not found");
            }
            return m_promptLibrary;
        }
    }

    private InputPrompt m_prompt;

    public event Action<InputPrompt> OnWindowOpened;
    public ReactiveWindow()
    {
        SetDefenderPrompts();
    }
    private void Reset() 
    {
        m_attackerReaction = ReactionType.None;
        m_defenderReaction = ReactionType.None;

        m_defenderTime = 0f;
        m_attackerTime = 0f;
    }
    public void Open(ActionContext ctx)
    {
        Reset();

        m_context = ctx;        

        m_prompt = PromptLibrary.Get(ctx.PromptKey);
        m_prompt.action.Enable();
        foreach (var dp in m_defenderPrompts)
        {
            dp.action.Enable();
        }
        
        OnParryWindowOpened?.Invoke(true);
        OnDodgeWindowOpened?.Invoke(true);
        OnConfirmWindowOpened?.Invoke(true);
        OnWindowOpened?.Invoke(m_prompt);

        m_windowOpen = true;
    }
    public void Close(ActionContext ctx)
    {
        m_windowOpen = false;
        OnWindowClosed?.Invoke(ctx);

        Reset();
        m_context = ctx;
        m_prompt = PromptLibrary.Get(ctx.PromptKey);
        m_prompt.action.Disable();
        foreach (var dp in m_defenderPrompts)
        {
            dp.action.Disable();
        }
        OnParryWindowOpened?.Invoke(false);
        OnDodgeWindowOpened?.Invoke(false);
        OnConfirmWindowOpened?.Invoke(false);
        OnWindowOpened?.Invoke(m_prompt);
    }
    #region DefenderPrompts
    public void SetDefenderPrompts()
    {
        m_defenderPrompts.Clear();

        AddPromptIfValid("DodgePrompt");
        AddPromptIfValid("ParryPrompt");
    }
    private void AddPromptIfValid(string key)
    {
        var prompt = PromptLibrary.Get(key);
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
    #endregion
    #region Input
    private void HandleInput()
    {
        HandleAttackerInput();
        HandleDefensiveInput();
    }
    private void HandleAttackerInput() 
    {
        if (m_context == null) 
        {
            Debug.Log("RW: Context is NULL");
            return;
        }
        if (!m_context.Source.IsPlayer) return;

        if (m_prompt.action.WasPressedThisFrame()) 
        {
            switch (m_prompt.inputType) 
            {
                case PromptInputType.Confirm:
                    TryActivateConfirm();
                    break;
            }
        }
    }
    private void HandleDefensiveInput() 
    {
        if (m_context == null)
        {
            Debug.Log("RW: Context is NULL");
            return;
        }
        if (m_context.Source.IsPlayer) return;

        for (int i = 0; i < m_defenderPrompts.Count; i++) 
        {
            InputPrompt p = m_defenderPrompts[i];
            if (p.action.WasPressedThisFrame()) 
            {
                switch (p.inputType) 
                {
                    case PromptInputType.Dodge:
                        TryActivateDodge();
                        break;
                    case PromptInputType.Parry:
                        TryActivateParry();
                        break;
                }
            }
        }
    }
    #endregion
    public void Update(float dt)
    {
        if (!m_windowOpen) return;
        HandleInput();

        m_attackerTime += dt;
        m_defenderTime += dt;

        if (m_parryCooldownTimer >= 0f)
            m_parryCooldownTimer -= dt;
        if (m_dodgeCooldownTimer >= 0f)
            m_dodgeCooldownTimer -= dt;
        if (m_confirmCooldownTimer >= 0f)
            m_confirmCooldownTimer -= dt;

        if (!CanParry)
            OnParryWindowOpened?.Invoke(false);
        if (!CanDodge)
            OnDodgeWindowOpened?.Invoke(false);
        if (!CanConfirm)
            OnConfirmWindowOpened?.Invoke(false);
    }

    public void TryActivateParry() 
    {
        if (!CanParry) return;

        m_defenderReaction = ReactionType.Parry;
        m_parryCooldownTimer = m_parryCooldown;
    }
    public void TryActivateDodge() 
    {
        if (!CanDodge) return;

        m_defenderReaction = ReactionType.Dodge;
        m_dodgeCooldownTimer = m_dodgeCooldown;
    }
    public void TryActivateConfirm()
    {
        if (!CanConfirm) return;

        m_attackerReaction = ReactionType.Confirm;
        m_confirmCooldownTimer = m_confirmCooldown;
    }

    public ReactionType ConsumeDefenderReaction() 
    {
        ReactionType result = m_defenderReaction;
        m_defenderReaction = ReactionType.None;
        return result;
    }
    public ReactionType ConsumeAttackerReaction()
    {
        var result = m_attackerReaction;
        m_attackerReaction = ReactionType.None;
        return result;
    }   
}
