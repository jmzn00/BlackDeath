using System.Collections.Generic;
using UnityEngine;

public class CombatCommandProcessor : CombatSystemBase
{
    private CombatCommandDispatcher m_dispatcher;
    private CombatContext m_context;
    public CombatCommandProcessor(CombatCommandDispatcher dispatcher) 
    {
        m_dispatcher = dispatcher;        
    }
    public override void Init(CombatContext context)
    {        
        m_context = context;

        foreach (var a in m_context.Actors)
        {
            a.ActionProvider.OnCommandReady += SubmitCommand;
            a.ReactionProvider.OnCommandReady += SubmitCommand;
        }
    }
    public override void Reset()
    {
        if (m_context == null || m_context.Actors == null) return;

        foreach (var a in m_context.Actors)
        {
            a.ActionProvider.OnCommandReady -= SubmitCommand;
            a.ReactionProvider.OnCommandReady -= SubmitCommand;
        }
        m_context = null;
    }
    private void SubmitCommand(ICombatCommand command) 
    {
        if (command.Source.IsDead) 
        {
            Debug.Log($"CP: Ignoring {command.Source.name} command. They are marked as dead");
            return;
        }
        m_dispatcher.Dispatch(command);
    }
}