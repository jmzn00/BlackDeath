using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public enum CombatState
{
    Active,
    Inactive,
    Starting,
    NextTurn,
    WaitingForAction,
    TurnStarting,
    Resolving,
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
    private TurnSystem m_turn;
    private ReactionSystem m_reaction;    
    private ActionSystem m_action;
    public ActionSystem Action => m_action;

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
        return true;
    }
    #endregion
    public void StartCombat(List<CombatActor> actors)
    {
        if (m_state != CombatState.Inactive) return;

        CombatEvents.CombatStarted();
        CombatEvents.CombatActorsChanged(actors);

        m_state = CombatState.Active;        

        m_context = new CombatContext(actors);
        m_turn = new TurnSystem(m_context);
        m_reaction = new ReactionSystem();
        m_action = new ActionSystem(m_context, m_reaction);

        m_action.OnActionFinished += ActionFinished;

        StartNextTurn();
    }
    private void ActionFinished(ActionContext aCtx) 
    {
        ActionResult result = m_reaction.ResolveResults();
        aCtx.Action.ResolveResult(aCtx, result);
        Debug.Log($"{aCtx.Source.name} Performed Action {aCtx.Action.actionName} on {aCtx.Target.name}. Result {result}");

        if (CheckEnd()) 
        {
            EndCombat();
        }
        else 
        {
            StartNextTurn();
        }
    }
    private void StartNextTurn() 
    {
        CombatActor actor = m_turn.Next();
        if (actor == null) 
        {
            Debug.LogWarning($"Turn System Next() == NULL");
            EndCombat();
            return;
        }
        CombatEvents.TurnEnded(m_context.CurrentActor);
        CombatEvents.TurnStarted(actor);

        m_context.SetCurrentActor(actor);
        m_context.AdvanceTurn();
        m_action.TurnStarted();
    }
    private bool CheckEnd()
    {
        var actors = m_context.Actors.ToList();

        bool playersAlive = actors.Exists(a => a.IsPlayer && !a.IsDead);
        bool enemiesAive = actors.Exists(e => !e.IsPlayer && !e.IsDead);

        return !playersAlive || !enemiesAive;            
    }
    private void EndCombat()
    {
        if (m_state == CombatState.Inactive) return;

        CombatEvents.CombatEnded(CombatResult.Won);
        m_state = CombatState.Inactive;
    }
}
