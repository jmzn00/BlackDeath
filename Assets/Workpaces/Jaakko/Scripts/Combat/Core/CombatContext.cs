using System.Collections.Generic;
public class CombatContext
{
    public IReadOnlyList<CombatActor> Actors => m_actors;
    public CombatActor CurrentActor { get; private set; }

    public int TurnNumber { get; private set; }
    public int RoundNumber { get; private set; }

    public CombatState State { get; private set; }

    private List<CombatActor> m_actors = new();

    public void Initialize(List<CombatActor> actors)
    {
        m_actors = actors;
        TurnNumber = 0;
        RoundNumber = 1;
        State = CombatState.Active;
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
    public void SetState(CombatState state)
    {
        State = state;
    }
}
