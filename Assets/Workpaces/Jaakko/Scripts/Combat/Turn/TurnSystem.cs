using UnityEngine;
using System.Collections.Generic;

public class TurnSystem
{
    List<CombatActor> m_actors;
    int m_turnIndex;

    public CombatActor Current => m_actors[m_turnIndex];
    private CombatContext m_context;

    public void Initialize(List<CombatActor> actors, CombatContext ctx) 
    {
        m_actors = actors;
        m_turnIndex = 0;
        m_context = ctx;

        CombatEvents.OnActionFinished += ActionFinished;
        CombatEvents.OnActorDied += ActorDied;
    }
    private void ActorDied(CombatActor deadActor) 
    {
        if (deadActor == Current) 
        {
            Next();
        }
    }
    private void ActionFinished(ActionContext ctx) 
    {
        if (ctx.Source != Current) 
        {
            Debug.Log("TS: Cannot Go To Next, Source != Current");
            return;
        }
        CombatActor next = Next();          
    }
    public CombatActor Start() 
    {
        if (m_actors.Count == 0)
            return null;
        if (m_actors[m_turnIndex].IsDead)
            return Next();
        CombatActor next = m_actors[m_turnIndex];
        m_context.SetCurrentActor(next);
        CombatEvents.TurnStarted(next);
        return next;
    }
    public CombatActor Next() 
    {
        int attemps = 0;
        do
        {
            m_turnIndex = (m_turnIndex + 1) % m_actors.Count;
            attemps++;
        }
        while (m_actors[m_turnIndex].IsDead && attemps < m_actors.Count);
        CombatActor next = m_actors[m_turnIndex];
        CombatEvents.TurnStarted(next);
        m_context.SetCurrentActor(next);
        m_context.AdvanceTurn();        
        return next;
    }
    public bool HasLivingActors() 
    {
        return m_actors.Exists(a => !a.IsDead);
    }

}
