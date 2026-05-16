using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;

public class ActionSystem : CombatSystemBase
{
    private ReactionSystem m_reaction;
    private CombatContext m_context;
    private CombatManager m_combat;

    private ActionContext m_currentAction;

    public event Action<ActionContext> OnActionFinished;
    public event Action<ActionContext> OnActionSubmitted;

    public event Action<ActionContext, ActionResult> OnActionResolved;
    public ActionSystem(
        ReactionSystem reaction, CombatManager combat)
    {
        m_reaction = reaction;
        m_combat = combat;
    }
    public override void Init(CombatContext context)
    {
        m_context = context;
        CombatEvents.OnTransitionEnded += Resolve;
    }
    public override void Reset()
    {
        CombatEvents.OnTransitionEnded -= Resolve;
        m_currentAction = null;
    }

    public void OpenPrompt(CombatActor actor, string promptKey)
    {
        if (m_currentAction == null)
        {
            Debug.LogWarning("AS: Cannot Open Prompt: Action Is NULL");
            return;
        }
        if (actor != m_currentAction.Source)
        {
            Debug.LogWarning("AS: actor != currentAction.Source");
            return;
        }
        CombatEvents.ActionStrikeMoment(m_currentAction);

        if (!m_currentAction.Action.isReactive)
            return;

        Debug.Log($"Open Window {promptKey}");
        m_currentAction.PromptKey = promptKey;
        m_reaction.Open(m_currentAction);
    }
    public void ClosePrompt(CombatActor actor)
    {
        if (m_currentAction == null)
        {
            Debug.LogWarning("AS: Cannot Close Prompt: Action Is NULL");
            return;
        }
        if (actor != m_currentAction.Source)
        {
            Debug.LogWarning("AS: Can not Close, Actor is not Source");
            return;
        }
        if (!m_currentAction.Action.isReactive)
            return;

        Debug.Log($"ReactionWindow Closed");
        m_reaction.Close();
        ActionResult res = m_reaction.ResolveResults();

        m_currentAction.ConfirmGrade = m_reaction.ResolveConfirmGrade();

        // Fire grade event only for player-attacker reactive actions (confirm window)
        if (m_currentAction.Source.Team == Team.Player
            && (res == ActionResult.Confirmed || res == ActionResult.Hit))
        {
            CombatEvents.ConfirmGraded(m_currentAction, m_currentAction.ConfirmGrade);
        }

        CombatEvents.ActionResolved(m_currentAction, res);
        OnActionResolved?.Invoke(m_currentAction, res);
    }

    public void NotifyActionFinished(CombatActor actor)
    {
        if (m_currentAction == null 
            || m_currentAction.Source != actor)
            return;

        ActionFinished(actor);
    }
    private void ActionFinished(CombatActor actor) 
    {
        if (actor != m_currentAction.Source)
        {
            Debug.LogWarning("AS: Can not Finish, Actor is not Source");
            return;
        }

        // for actions that have no prompt thus they
        // do not open / close the window

        Debug.Log($"{m_currentAction.Action.actionName} finished. Reactive: {m_currentAction.Action.isReactive}");
        if (!m_currentAction.Action.isReactive) 
        {
            OnActionResolved?.Invoke(m_currentAction, ActionResult.Confirmed);
            CombatEvents.ActionResolved(m_currentAction, ActionResult.Confirmed);
        }
        
        CombatEvents.TurnEnded(m_currentAction.Source);
        CombatEvents.ActionFinished(m_currentAction);

        OnActionFinished?.Invoke(m_currentAction);
        m_currentAction = null;
    }
    public void TurnStarted()
    {
        m_combat.ChangeState(CombatState.Action);

        m_context.CurrentActor.ActionProvider.
            RequestAction(m_context.CurrentActor, m_context.Actors.ToList());
    }
    public void Resolve()
    {
        if (m_currentAction == null)
        {
            Debug.LogWarning($"Trying to Resolve Null Action");
            return;
        }
        m_currentAction.Action.Resolve(m_currentAction);
    }
    public void SubmitAction(AttackCommand attackCommand)
    {
        if (m_currentAction != null)
        {
            Debug.LogWarning("Cannot Submit Action, Action already Set");
            return;
        }

        CombatAction action = attackCommand.Action;
        CombatActor source = attackCommand.Source;
        List<CombatActor> targets = attackCommand.Targets;

        m_currentAction = new ActionContext
        {
            Source = attackCommand.Source,
            Action = attackCommand.Action,
            Targets = attackCommand.Targets
        };
        if (action == null)
        {
            OnActionFinished?.Invoke(m_currentAction);
            return;
        }

        source.RemoveActionPoints(action.apCost);
        OnActionSubmitted?.Invoke(m_currentAction);
        CombatEvents.ActionSubmitted(m_currentAction);
    }
}