using System;
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

    private float m_elapsed;
    private float m_confirmPressedAt = -1f;
    private float m_perfectFraction;

    public event Action<ActionContext> OnWindowClosed;

    public void Open(float perfectFraction = 0.65f)
    {
        m_attackerReaction = ReactionType.None;
        m_defenderReaction = ReactionType.None;
        m_elapsed = 0f;
        m_confirmPressedAt = -1f;
        m_perfectFraction = perfectFraction;
        m_windowOpen = true;
    }

    public void Tick(float dt)
    {
        if (m_windowOpen) m_elapsed += dt;
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
        if (m_confirmPressedAt < 0)
            m_confirmPressedAt = m_elapsed;
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

    public ConfirmGrade GetConfirmGrade()
    {
        if (m_confirmPressedAt < 0)
            return ConfirmGrade.Missed;

        if (m_elapsed <= 0f)
            return ConfirmGrade.Good;

        float fraction = m_confirmPressedAt / m_elapsed;
        return fraction >= m_perfectFraction ? ConfirmGrade.Perfect : ConfirmGrade.Good;
    }
}
