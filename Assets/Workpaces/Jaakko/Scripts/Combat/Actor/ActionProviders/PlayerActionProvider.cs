using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerActionProvider : IActionProvider
{
    public PlayerActionProvider() 
    {
    }
    private ICombatCommand m_command;
    public event Action<AttackCommand> OnCommandReady;
    public void Begin(CombatActor actor, List<CombatActor> participants) 
    {
        m_command = null;
    }
    public ICombatCommand GetCommand() 
    {
        return m_command;
    }
    public void RequestAction(CombatActor actor,
        List<CombatActor> participants) 
    {
        
    }
    // ui sets
    public void SetAction(ActionContext ctx) 
    {
        OnCommandReady?.Invoke(new AttackCommand(ctx.Source, ctx.Target, ctx.Action));
        //ctx.Source.SubmitAction(ctx.Source, ctx.Target, ctx.Action);
    }    
}
