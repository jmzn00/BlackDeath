using UnityEngine;
using System.Collections.Generic;

public class PlayerActionProvider : IActionProvider
{
    public PlayerActionProvider() 
    {
    }
    public void RequestAction(CombatActor actor,
        List<CombatActor> participants) 
    {
        
    }
    // ui sets
    public void SetAction(ActionContext ctx) 
    {
        ctx.Source.SubmitAction(ctx.Source,
                ctx.Target, ctx.Action);
    }    
}
