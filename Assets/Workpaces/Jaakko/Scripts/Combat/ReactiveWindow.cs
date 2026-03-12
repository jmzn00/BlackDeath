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
  
    private ReactionType m_attackerReaction;
    private ReactionType m_defenderReaction;

    public event Action<bool> OnParryWindowOpened;
    public event Action<bool> OnDodgeWindowOpened;
    public event Action<bool> OnConfirmWindowOpened;

    public event Action<ActionContext> OnWindowClosed;

    private ActionContext m_context;

    private List<InputPrompt> m_defenderPrompts = new();
    private InputPrompt m_attackerPrompt;

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
    

    public event Action<InputPrompt> OnWindowOpened;
    public ReactiveWindow()
    {
        SetDefenderPrompts();
    }
    private void Reset() 
    {
        m_attackerReaction = ReactionType.None;
        m_defenderReaction = ReactionType.None;
    }
    public void Open(ActionContext ctx)
    {
        Reset();
        m_context = ctx;   
        m_skipFirstFrame = true;

        m_attackerPrompt = PromptLibrary.Get(ctx.PromptKey);
        m_attackerPrompt.action.Enable();
        foreach (var dp in m_defenderPrompts)
        {
            dp.action.Enable();
        }
        
        OnParryWindowOpened?.Invoke(true);
        OnDodgeWindowOpened?.Invoke(true);
        OnConfirmWindowOpened?.Invoke(true);
        OnWindowOpened?.Invoke(m_attackerPrompt);

        m_windowOpen = true;
    }
    public void Close(ActionContext ctx)
    {        
        m_windowOpen = false;
        OnWindowClosed?.Invoke(ctx);

        Reset();
        m_context = ctx;

        // band-aid fix for skip action
        if (m_context.PromptKey != null && ctx.Prompt != null) 
        {
            m_attackerPrompt = PromptLibrary.Get(ctx.PromptKey);
            m_attackerPrompt.action.Disable();
            foreach (var dp in m_defenderPrompts)
            {
                dp.action.Disable();
            }
        }
        
        OnParryWindowOpened?.Invoke(false);
        OnDodgeWindowOpened?.Invoke(false);
        OnConfirmWindowOpened?.Invoke(false);
        OnWindowOpened?.Invoke(m_attackerPrompt);

        OnConfirmWindowOpened?.Invoke(false);
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
    private bool m_skipFirstFrame;    
    public void Update(float dt)
    {        
        if (!m_windowOpen) return;

        if (m_skipFirstFrame == true)
        {
            m_skipFirstFrame = false;
            return;
        }

        if (m_attackerReaction == ReactionType.None)
            m_context.Source.ReactionProvider.TryReact(this, m_attackerPrompt);
        if (m_defenderReaction == ReactionType.None) 
        {
            for (int i = 0; i < m_defenderPrompts.Count; i++)
            {
                m_context.Target.ReactionProvider.TryReact(this, m_defenderPrompts[i]);
            }
        }        
    }

    public void TryActivateParry() 
    {
        m_defenderReaction = ReactionType.Parry;
    }
    public void TryActivateDodge() 
    {
        m_defenderReaction = ReactionType.Dodge;
    }
    public void TryActivateConfirm()
    {
        m_attackerReaction = ReactionType.Confirm;
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
