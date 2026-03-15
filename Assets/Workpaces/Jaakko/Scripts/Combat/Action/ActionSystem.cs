using System;
using System.Linq;
using UnityEngine;

public class ActionSystem
{
    private ReactionSystem m_reaction;
    private CombatContext m_context;

    private ActionContext m_currentAction;

    public event Action<ActionContext> OnActionFinished;
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
        m_currentAction.Action.ResolveResult(m_currentAction, res);
    }

    public void NotifyActionFinished(CombatActor actor) 
    {
        if (m_currentAction == null)
            return;
        if (actor != m_currentAction.Source) 
        {
            // if you get this warning it may be that ui is trying to submit again 
            // when you are confirming an attack

            Debug.LogWarning("AS: Can not Finish, Actor is not Source");
            return;
        }        
        OnActionFinished?.Invoke(m_currentAction);
        CombatEvents.ActionFinished(m_currentAction);
        CombatEvents.TurnEnded(m_currentAction.Source);
        m_currentAction = null;
    }
    public void TurnStarted() 
    {
        m_context.CurrentActor.ActionProvider.
            RequestAction(m_context.CurrentActor, m_context.Actors.ToList());
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
        action.Resolve(m_currentAction, TempAction); // REMOVE TEMP
        CombatEvents.ActionSubmitted(m_currentAction);
    }
    public event Action TempAction; // REMOVE
}
