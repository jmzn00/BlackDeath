using System.Collections.Generic;

public class TurnSystem : CombatSystemBase
{
    IReadOnlyList<CombatActor> m_actors;
    int m_turnIndex;

    private CombatContext m_context;
    public TurnSystem()
    {
        
    }
    public override void Init(CombatContext context)
    {
        m_context = context;
        m_turnIndex = 0;
        m_actors = m_context.Actors;
    }
    public override void Reset()
    {
        m_context = null;
        m_turnIndex = 0;
        m_actors = null;
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
