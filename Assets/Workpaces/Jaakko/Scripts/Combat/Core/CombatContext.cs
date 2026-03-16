using System.Collections.Generic;
public class CombatContext
{    
    public IReadOnlyList<CombatActor> Actors => m_actors;
    public CombatActor CurrentActor { get; private set; }
    public int TurnNumber { get; private set; }
    public int RoundNumber { get; private set; }

    private List<CombatActor> m_actors = new();
    public CombatContext(List<CombatActor> actors)
    {
        m_actors = actors;
    }
    public void SetCurrentActor(CombatActor actor)
    {
        CurrentActor = actor;
    }
    public void AdvanceTurn()
    {
        TurnNumber++;

        if (TurnNumber % m_actors.Count == 0)
            RoundNumber++;
    }    
}
