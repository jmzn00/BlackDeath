using System;
using System.Linq;
using UnityEngine;

public class ActionSystem
{
    private ReactionSystem m_reaction;
    private TurnSystem m_turn;
    private CombatContext m_context;

    private ActionContext m_currentAction;

    public void OpenPrompt(string promptKey) 
    {
        if (m_currentAction == null) 
        {
            Debug.LogWarning("AS: Cannot Open Prompt: Action Is NULL");
            return;
        }
        m_currentAction.PromptKey = promptKey;
        m_reaction.Open(m_currentAction);
    }
    public void ClosePrompt() 
    {
        if (m_currentAction == null) 
        {
            Debug.LogWarning("AS: Cannot Close Prompt: Action Is NULL");
            return;
        }        
        m_reaction.Close();
    }
    public void NotifyActionFinished(CombatActor actor) 
    {
        if (m_currentAction == null)
            return;
        if (actor != m_currentAction.Source) 
        {
            Debug.LogWarning("AS: Cannot Finish, Actor is not Source");
            return;
        }
        CombatEvents.ActionFinished(m_currentAction);
        
        m_currentAction = null;
    }

    public void Initialize(CombatContext ctx,
        TurnSystem turns,
        ReactionSystem reaction)
    {
        m_context = ctx;
        m_turn = turns;
        m_reaction = reaction;

        CombatEvents.OnReactionResolved += HandleReactionResolved;
        CombatEvents.OnTurnStarted += TurnStarted;
    }
    private void TurnStarted(CombatActor actor) 
    {
        actor.ActionProvider.RequestAction(actor, m_context.Actors.ToList());
    }
    public void SubmitAction(CombatActor source, 
        CombatActor target,
        CombatAction action)
    {
        if (m_currentAction != null) 
        {
            Debug.LogWarning("Cannot Submit Action, Action already Set");
            return;
        }

        m_currentAction = new ActionContext
        {
            Source = source,
            Target = target,
            Action = action,
        };
        Debug.Log($"{source.name} Submitted Action: {action.actionName}");

        action.Resolve(m_currentAction, TempAction);
    }
    public event Action TempAction;
    private void HandleReactionResolved(ActionContext context, ActionResult result) 
    {
        if (context != m_currentAction) 
        {
            Debug.LogWarning("AS: Cannot Resolve, context != currentAction");
            return;
        }
        context.Action.ResolveResult(context, result); 
        CombatEvents.ActionResolved(context, result);
    }
}
