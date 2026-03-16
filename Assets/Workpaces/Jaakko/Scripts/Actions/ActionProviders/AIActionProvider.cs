using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AIActionProvider : IActionProvider
{
    private float m_waitTime = 2f;
    private Coroutine m_coroutine;
    private bool m_hasActed;

    private AICombatActor m_actor;
    public AIActionProvider(AICombatActor aiActor) 
    {
        m_actor = aiActor;
    }
    public void RequestAction(CombatActor actor, List<CombatActor> participants) 
    {
        if (m_hasActed)
            return;

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

        float bestActionScore = -1f;
        CombatAction bestAction = null;
        for (int i = 0; i < m_actor.ActionBehaviours.Count; i++) 
        {
            float score;
            AIActionBehaviour b = m_actor.ActionBehaviours[i];
            score = b.Evaluate(actor, participants, out CombatAction action);

            if (score > bestActionScore) 
            {
                bestActionScore = score;
                bestAction = action;
            }
        }
        float bestTargetScore = -1f;
        CombatActor bestTarget = null;
        for (int i = 0; i < m_actor.TargetingBehaviours.Count; i++) 
        {
            AITargetingBehaviour tb = m_actor.TargetingBehaviours[i];
            float score = tb.Evaluate(actor, participants, out CombatActor target);

            if (score > bestTargetScore) 
            {
                bestTargetScore = score;
                bestTarget = target;
            }
        }
        ActionContext ctx = null;
        if (bestAction != null && bestTarget != null) 
        {
            ctx = new ActionContext()
            {
                Action = bestAction,
                Target = bestTarget
            };
        }
        m_actor.SubmitAction(m_actor, bestTarget, bestAction);                
        m_coroutine = null;
        m_hasActed = false;
    }
}
