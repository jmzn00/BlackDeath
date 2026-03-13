using UnityEngine;
using System.Collections.Generic;

public class TurnSystem
{
    IReadOnlyList<CombatActor> m_actors;
    int m_turnIndex;

    public CombatActor Current => m_actors[m_turnIndex];
    private CombatContext m_context;
    public TurnSystem(CombatContext ctx)
    {
        m_context = ctx;
        m_turnIndex = 0;
        m_actors = m_context.Actors;

        Start();
    }   
    public CombatActor Start() 
    {
        if (m_actors.Count == 0)
            return null;

        CombatActor next = m_actors[m_turnIndex];

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

        return next;
    }
}
