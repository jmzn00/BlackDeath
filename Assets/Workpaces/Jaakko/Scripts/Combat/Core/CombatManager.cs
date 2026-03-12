using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public enum CombatState
{
    Active,
    Inactive
}
public enum CombatResult 
{
    Won,
    Lost
}
public class CombatManager : IManager
{
    private CombatState m_state = CombatState.Inactive;
    public CombatState State => m_state;

    private GameManager m_game;

    private CombatContext m_context;
    private TurnSystem m_turnSystem;
    private ReactiveWindow m_window;
    private ReactionSystem m_reaction;
    private ActionSystem m_action;
    public ActionSystem Action => m_action;

    private List<CombatActor> m_actors;

    public CombatManager(GameManager game)
    {
        m_game = game;      
    }
    #region IManager
    public void Update(float dt)
    {
        if (m_state == CombatState.Inactive) return;

        m_reaction.Update(dt);
    }
    public bool Init()
    {
        m_context = new CombatContext();
        m_turnSystem = new TurnSystem();
        m_window = new ReactiveWindow();
        m_reaction = new ReactionSystem();
        m_action = new ActionSystem();

        CombatEvents.OnActorDied += ActorDied;

        return true;
    }
    public void OnManagersInitialzied() 
    {
        List<CombatArea> areas =
            GameObject.
            FindObjectsByType<CombatArea>
            (FindObjectsSortMode.None).
            ToList();
        foreach (CombatArea area in areas) 
        {
            area.Initialize(m_game);
        }
    }
    public bool Dispose()
    {
        CombatEvents.OnActorDied -= ActorDied;
        return true;
    }
    #endregion
    public void StartCombat(List<CombatActor> actors) 
    {
        if (m_state != CombatState.Inactive) return;

        m_state = CombatState.Active;
        m_actors = actors;

        m_context.Initialize(actors);
        m_turnSystem.Initialize(actors, m_context);

        m_reaction.Initialize(m_window);
        m_action.Initialize(m_context,
            m_turnSystem,
            m_reaction);        
        CombatEvents.CombatStarted();
        CombatEvents.CombatActorsChanged(actors);        

        CombatActor first = m_turnSystem.Start();
        m_context.SetCurrentActor(first);     
    }
    private void EndCombat()
    {
        if (m_state == CombatState.Inactive) return;

        CombatEvents.CombatEnded(CombatResult.Won);
        m_state = CombatState.Inactive;
    }
    private void ActorDied(CombatActor actor) 
    {
        if (CheckEnd()) 
        {
            EndCombat();
        }
    }
    private bool CheckEnd() 
    {
        if (!m_actors.Exists(a => a.IsPlayer && !a.IsDead)) 
        {
            return true;
        }
        if (!m_actors.Exists(a => !a.IsPlayer && !a.IsDead)) 
        {
            return true;
        }
        return false;
    }

}
