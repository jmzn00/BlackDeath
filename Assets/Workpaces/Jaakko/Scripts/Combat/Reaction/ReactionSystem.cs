using System;
using System.Collections.Generic;
using UnityEngine;
public class ReactionSystem
{
    private InputPromptLibrary m_promptLibrary;
    private InputPromptLibrary Library 
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
    private ReactiveWindow m_window;
    private ActionContext m_context;

    private List<InputPrompt> m_defensivePrompts = new();

    public ReactionSystem() 
    {
        m_window = new ReactiveWindow();

        InputPrompt parryPrompt = Library.Get("ParryPrompt");
        InputPrompt dodgePrompt = Library.Get("DodgePrompt");

        if (!m_defensivePrompts.Contains(parryPrompt))
        {
            m_defensivePrompts.Add(parryPrompt);
        }
        if (!m_defensivePrompts.Contains(dodgePrompt))
        {
            m_defensivePrompts.Add((dodgePrompt));
        }
    }
    public void Open(ActionContext ctx)
    {
        if (m_window.IsOpen) 
        {
            Debug.LogWarning("Cannot Open: Window allready open");
            return;
        }
        m_context = ctx;
        InputPrompt attackerPrompt = Library.Get(ctx.PromptKey);
        m_context.Prompt = attackerPrompt;

        // this can be null if the animation calls open with an empty prompt
        if (m_context.Prompt != null) 
            m_context.Prompt.action.Enable();

        foreach (var p in m_defensivePrompts)
            p.action.Enable();        

        m_window.Open();
        CombatEvents.ReactionWindowOpened(ctx);
    }
    public void Close() 
    {
        if (!m_window.IsOpen) 
        {
            Debug.LogWarning("Cannot Close: Window Is Not Open");
            return;
        }

        // this can be null if the animation calls open with an empty prompt
        if (m_context.Prompt != null)
            m_context.Prompt.action.Disable();

        
        foreach (var p in m_defensivePrompts)
            p.action.Disable();

        m_window.Close(m_context);
        CombatEvents.ReactionWindowClosed(m_context);
    }
    public void Update(float dt) 
    {
        if (!m_window.IsOpen) return;

        m_context.Source.ReactionProvider.TryReact(this, m_context.Prompt);

        for (int i = 0; i < m_defensivePrompts.Count; i++)
        {
            InputPrompt p = m_defensivePrompts[i];
            m_context.Target.ReactionProvider.TryReact(this, p);
        }
    }
    public void ReceiveReaction(CombatActor actor, InputPrompt prompt) 
    {
        if (!m_window.IsOpen) 
        {
            Debug.LogWarning("Cannot recive reaction while window is closed");
            return;
        }
        if (actor == m_context.Target
            && m_window.ConsumeDefenderReaction() != ReactionType.None)
            return;

        switch (prompt.inputType) 
        {
            case PromptInputType.Parry:
                m_window.TryActivateParry();
                break;
            case PromptInputType.Dodge:
                m_window.TryActivateDodge();
                break;
            case PromptInputType.Confirm:
                m_window.TryActivateConfirm();
                break; 
        }
    }
    public ActionResult ResolveResults()
    {
        ReactionType attacker = m_window.ConsumeAttackerReaction();
        ReactionType defender = m_window.ConsumeDefenderReaction();


        if (defender == ReactionType.Parry)
            return ActionResult.Parried;
        if (defender == ReactionType.Dodge)
            return ActionResult.Dodged;
        if (attacker == ReactionType.Confirm)
            return ActionResult.Confirmed;

        return ActionResult.Hit;
    }
}
