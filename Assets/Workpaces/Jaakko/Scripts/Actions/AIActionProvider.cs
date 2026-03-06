using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIActionProvider : IActionProvider
{
    private float m_waitTime = 2f;
    private Coroutine m_coroutine;
    public void RequestAction(CombatActor actor, List<CombatActor> participants) 
    {
        if (m_coroutine == null)
            m_coroutine = actor.StartCoroutine(WaitAndAct(actor, participants));
    }
    public void SetAction(ActionContext ctx) 
    {
        Debug.LogWarning("Trying To Externally Set Action On AIActionProvider");
    }
    private IEnumerator WaitAndAct(CombatActor actor, List<CombatActor> participants) 
    {
        yield return new WaitForSeconds(m_waitTime);

        CombatActor target = participants.Find(a => a.IsPlayer);

        CombatAction action = actor.Actions[0];

        ActionContext ctx = new ActionContext
        {
            Source = actor,
            Target = target,
            Action = action,
        };
        actor.SetActionContext(ctx);
        m_coroutine = null;
    }
}
