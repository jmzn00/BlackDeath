using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
public enum CombatState
{
    Active,
    Inactive,

    Transition,
    Action

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
    private TransitionSystem m_transition;
    public ActionSystem Action => m_action;

    public event Action OnCombatStarted;
    public event Action<CombatResult> OnCombatEnded;
    public event Action<CombatActor> OnTurnStart;
    public event Action<CombatActor> OnTurnEnd;

    private CombatArea m_area;

    public CombatManager(GameManager game)
    {
        m_game = game;
    }
    #region IManager
    public void Update(float dt)
    {
        if (m_state == CombatState.Inactive) return;

        m_reaction.Update(dt);
        m_transition.Update(dt);
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
        if (m_action != null) 
        {
            m_action.OnActionFinished -= ActionFinished;
            m_action.OnActionSubmitted -= ActionSubmitted;
        }
        if (m_transition != null) 
        {
            m_transition.OnTransitionFinished -= TransitionFinished;
        }                
        return true;
    }
    #endregion
    public void StartCombat(List<CombatActor> actors, CombatArea area)
    {
        if (m_state != CombatState.Inactive) return;

        CombatEvents.CombatActorsChanged(actors);

        m_area = area;
        ChangeState(CombatState.Active);
        m_game.SetState(GameState.Combat);
        OnCombatStarted?.Invoke();

        m_context = new CombatContext(actors);
        m_turn = new TurnSystem(m_context);
        m_reaction = new ReactionSystem();

        m_action = new ActionSystem(m_context, m_reaction);
        m_action.OnActionFinished += ActionFinished;
        m_action.OnActionSubmitted += ActionSubmitted;

        m_transition = new TransitionSystem(area);
        m_transition.OnTransitionFinished += TransitionFinished;
           
        NextTurn();
    }
    private void ActionSubmitted(ActionContext actx)
    {
        m_transition.Start(actx);
        ChangeState(CombatState.Transition);

        CombatEvents.ActionSubmitted(actx);
    }
    private void ActionFinished(ActionContext aCtx)
    {
        OnTurnEnd?.Invoke(aCtx.Source);
        CombatEvents.TurnEnded(aCtx.Source);        

        // returns players to their original positions
        m_transition.Reset();

        if (CheckEnd())
        {
            EndCombat();
        }
        else
        {
            NextTurn();
            ChangeState(CombatState.Active);
        }
    }
    private void TransitionFinished() 
    {
        ChangeState(CombatState.Action);
        m_action.Resolve();
    }
    private void NextTurn()
    {
        CombatActor actor = m_turn.Next();
        if (actor == null)
        {
            Debug.LogWarning($"Turn System Next() == NULL");
            EndCombat();
            return;
        }
        CombatEvents.TurnStarted(actor);
        OnTurnStart?.Invoke(actor);

        m_context.SetCurrentActor(actor);
        m_context.AdvanceTurn();
        m_action.TurnStarted();
    }
    private bool CheckEnd()
    {
        var actors = m_context.Actors.ToList();

        bool alliesAlieve = actors.Exists(a => a.Team == Team.Player && !a.IsDead);
        bool enemiesAive = actors.Exists(e => e.Team == Team.Enemy && !e.IsDead);

        return !alliesAlieve || !enemiesAive;
    }
    private void EndCombat()
    {
        if (m_state == CombatState.Inactive) return;
        m_state = CombatState.Inactive;

        CombatResult result = CombatResult.Lost;
        if (m_context.Actors.ToList().Exists(a => a.Team == Team.Player
        && !a.IsDead)) 
        {
            result = CombatResult.Won;
        }

        m_area.EndBattle(result);
        OnCombatEnded?.Invoke(result);
        ChangeState(CombatState.Inactive);
        CombatEvents.CombatEnded(result);        

        m_area = null;
    }
    public void ChangeState(CombatState state) 
    {
        if (state == m_state) return;

        m_state = state;
        CombatEvents.CombatStateChanged(m_state);
    }
}
