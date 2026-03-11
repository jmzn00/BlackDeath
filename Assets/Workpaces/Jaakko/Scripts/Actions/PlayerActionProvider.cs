using UnityEngine;
using System.Collections.Generic;

public class PlayerActionProvider : IActionProvider
{
    private CombatManager m_combatManager;
    public PlayerActionProvider(CombatManager manager, UIController uiController) 
    {
        m_combatManager = manager;
    }
    private ActionContext m_pendingAction;
    public void RequestAction(CombatActor actor, List<CombatActor> participants) 
    {
        //Debug.Log("Requesting Player Input");
        if (m_pendingAction != null) 
        {
            actor.SetActionContext(m_pendingAction);
            m_pendingAction = null;
        }
    }
    // ui sets
    public void SetAction(ActionContext ctx) 
    {
        m_pendingAction = ctx;
    }    
}
