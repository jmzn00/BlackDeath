using System.Collections.Generic;
using UnityEngine;

public class ReactionSystem : CombatSystemBase
{
    private InputPromptLibrary m_promptLibrary;
    private InputPromptLibrary Library
    {
        get
        {
            if (m_promptLibrary == null)
            {
                m_promptLibrary = Resources.Load<InputPromptLibrary>("InputPrompts/PromptLibrary");
                if (m_promptLibrary == null)
                    Debug.LogError("InputPromptLibrary not found");
            }
            return m_promptLibrary;
        }
    }

    private ReactiveWindow m_window;
    private ActionContext m_context;

    private List<InputPrompt> m_defensivePrompts = new();
    private List<InputPrompt> m_confirmPool = new();

    public ReactionSystem()
    {
        m_window = new ReactiveWindow();

        InputPrompt parryPrompt = Library.Get("ParryPrompt");
        InputPrompt dodgePrompt = Library.Get("DodgePrompt");

        if (!m_defensivePrompts.Contains(parryPrompt))
            m_defensivePrompts.Add(parryPrompt);
        if (!m_defensivePrompts.Contains(dodgePrompt))
            m_defensivePrompts.Add(dodgePrompt);

        if (Library.confirmPool != null)
            m_confirmPool.AddRange(Library.confirmPool);
    }

    public void Open(ActionContext ctx)
    {
        if (m_window.IsOpen)
        {
            Debug.LogWarning("Cannot Open: Window already open");
            return;
        }

        m_context = ctx;
        m_window.Open(ctx.Action.confirmPerfectFraction);

        // Pick attacker prompt: random from pool for player attackers, library key otherwise
        if (ctx.Source.Team == Team.Player && m_confirmPool.Count > 0)
        {
            m_context.Prompt = m_confirmPool[Random.Range(0, m_confirmPool.Count)];
        }
        else
        {
            m_context.Prompt = Library.Get(ctx.PromptKey);
        }

        if (m_context.Prompt != null)
        {
            m_context.Prompt.action.Enable();
            if (ctx.Source.Team == Team.Player)
                CombatEvents.AttackerPromptOpened(m_context.Prompt);
        }

        TargetType t = ctx.Action.targetType;

        if (t == TargetType.Enemy || t == TargetType.AOEEnemy)
        {
            if (ctx.Source.Team == Team.Enemy)
            {
                foreach (var p in m_defensivePrompts)
                {
                    p.action.Enable();
                    CombatEvents.DefenderPromptOpened(p);
                }
            }
        }

        if (ctx.Source != null)
            ctx.Source.ReactionProvider.OpenReaction();
        if (ctx.PrimaryTarget != null)
            ctx.PrimaryTarget.ReactionProvider.OpenReaction();

        CombatEvents.ReactionWindowOpened(ctx);
    }

    public void Close()
    {
        if (!m_window.IsOpen)
        {
            Debug.LogWarning("Cannot Close: Window Is Not Open");
            return;
        }

        if (m_context.Prompt != null)
            m_context.Prompt.action.Disable();

        foreach (var p in m_defensivePrompts)
            p.action.Disable();

        m_window.Close(m_context);
        CombatEvents.ReactionWindowClosed(m_context);
    }

    public override void Update(float dt)
    {
        if (!m_window.IsOpen) return;

        m_window.Tick(dt);

        m_context.Source.ReactionProvider.TryReact(this, m_context.Prompt);

        for (int i = 0; i < m_defensivePrompts.Count; i++)
        {
            InputPrompt p = m_defensivePrompts[i];
            if (m_context.PrimaryTarget != null)
                m_context.PrimaryTarget.ReactionProvider.TryReact(this, p);
        }
    }

    public void ReceiveReaction(ReactionCommand command)
    {
        CombatActor actor = command.Source;
        InputPrompt prompt = command.Prompt;

        if (!m_window.IsOpen)
        {
            Debug.LogWarning("Cannot receive reaction while window is closed");
            return;
        }
        if (actor == m_context.PrimaryTarget
            && m_window.ConsumeDefenderReaction() != ReactionType.None)
            return;

        switch (prompt.inputType)
        {
            case PromptInputType.Parry:
                m_window.TryActivateParry();
                break;
            case PromptInputType.Dodge:
                m_window.TryActivateDodge();
                CombatEvents.DodgeAttempted(m_context);
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

    public ConfirmGrade ResolveConfirmGrade() => m_window.GetConfirmGrade();
}
