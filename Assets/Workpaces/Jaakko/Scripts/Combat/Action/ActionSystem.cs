using System;
using System.Linq;
using UnityEngine;

public class ActionSystem : CombatSystemBase
{
    private ReactionSystem m_reaction;
    private CombatContext m_context;

    private ActionContext m_currentAction;

    public event Action<ActionContext> OnActionFinished;
    public event Action<ActionContext> OnActionSubmitted;

    public event Action<ActionContext, ActionResult> OnActionResolved;
    public ActionSystem(CombatContext ctx,
        ReactionSystem reaction)
    {
        m_reaction = reaction;
        m_context = ctx;
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
        m_reaction.Close();

        ActionResult res = m_reaction.ResolveResults();
        OnActionResolved?.Invoke(m_currentAction, res);
    }

    public void NotifyActionFinished(CombatActor actor)
    {
        if (m_currentAction == null)
            return;
        if (actor != m_currentAction.Source)
        {
            // if you get this warning it may be that ui is trying to submit again 
            // when you are confirming an attack or submiting input

            Debug.LogWarning("AS: Can not Finish, Actor is not Source");
            return;
        }

        // this is a temp fix for ally / self target skills
        // as they dont currently open / close the window

        if (m_currentAction.Action.targetType == TargetType.Self ||
            m_currentAction.Action.targetType == TargetType.Ally)
        {
            OnActionResolved?.Invoke(m_currentAction, ActionResult.Confirmed);

            //CombatEvents.ActionResolved(m_currentAction, ActionResult.Confirmed);
        }

        OnActionFinished?.Invoke(m_currentAction);
        m_currentAction = null;
    }
    public void TurnStarted()
    {
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
        CombatActor target = attackCommand.Target;

        source.RemoveActionPoints(action.apCost);

        if (action == null)
        {
            OnActionSubmitted?.Invoke(new ActionContext
            {
                Source = source
            });
            return;
        }

        m_currentAction = new ActionContext
        {
            Source = source,
            Target = target,
            Action = action,
        };
        OnActionSubmitted?.Invoke(m_currentAction);
    }
}