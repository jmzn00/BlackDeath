using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIActionProvider : IActionProvider
{
    private float m_waitTime = 2f;
    private Coroutine m_coroutine;
    private bool m_hasActed;
    public void RequestAction(CombatActor actor, List<CombatActor> participants) 
    {
        if (m_hasActed)
            return;

        //Debug.Log($"Request Action from {actor.name}");
        if (m_coroutine == null)
            m_coroutine = actor.StartCoroutine(WaitAndAct(actor, participants));
    }
    public void SetAction(ActionContext ctx) 
    {
        Debug.LogWarning("Trying To Externally Set Action On AIActionProvider");
    }
    private IEnumerator WaitAndAct(CombatActor actor, List<CombatActor> participants) 
    {
        m_hasActed = true;

        yield return new WaitForSeconds(m_waitTime);
        /*
        CombatActor target = participants.Find(a => a.IsPlayer
        && !a.IsDead);
        */
        List<CombatActor> targetActors
            = participants.FindAll(a => a.IsPlayer && !a.IsDead);

        CombatActor target = targetActors
            .ElementAt(Random.Range(0, targetActors.Count));

        //Debug.Log($"Selected Target{target.name}");

        CombatAction action = actor.Actions[0];

        ActionContext ctx = new ActionContext
        {
            Source = actor,
            Target = target,
            Action = action,
        };
        actor.SetActionContext(ctx);
        //Debug.Log($"{actor.name} Set Action");
        m_coroutine = null;
        m_hasActed = false;
    }
}
