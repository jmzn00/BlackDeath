using System.Collections.Generic;
using UnityEngine;

public class CombatCommandProcessor 
{
    private CombatCommandDispatcher m_dispatcher;
    private List<CombatActor> m_actors;
    public CombatCommandProcessor(List<CombatActor> actors, CombatCommandDispatcher dispatcher) 
    {
        m_dispatcher = dispatcher;
        m_actors = actors;

        foreach (var a in m_actors) 
        {
            a.ActionProvider.OnCommandReady += SubmitCommand;
            a.ReactionProvider.OnCommandReady += SubmitCommand;
        }
    }
    public void Dispose() 
    {
        foreach (var a in m_actors) 
        {
            a.ActionProvider.OnCommandReady -= SubmitCommand;
            a.ReactionProvider.OnCommandReady-= SubmitCommand;
        }
    }
    private void SubmitCommand(ICombatCommand command) 
    {
        if (command.Source.IsDead) 
        {
            Debug.Log($"CP: Ignoring {command.Source.name} command. They are marked as dead");
            return;
        }
        //Debug.Log($"{command.Source.name} submitted command {command}");
        m_dispatcher.Dispatch(command);
    }
}