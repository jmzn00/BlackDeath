using System;
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
    private bool m_windowOpen;
    public bool IsOpen => m_windowOpen;

    private ReactionType m_attackerReaction;
    private ReactionType m_defenderReaction;

    public event Action<ActionContext> OnWindowClosed;

    public void Open()
    {
        m_attackerReaction = ReactionType.None;
        m_defenderReaction = ReactionType.None;

        m_windowOpen = true;
    }

    public void Close(ActionContext ctx)
    {
        m_windowOpen = false;
        OnWindowClosed?.Invoke(ctx);
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
        var result = m_defenderReaction;
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
