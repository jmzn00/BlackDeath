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
[Serializable]
public class CombatSaveData 
{
    public List<string> CompletedAreas = new();
}
public class CombatManager : ManagerBase
{
    private CombatState m_state = CombatState.Inactive;
    public CombatState State => m_state;

    private GameManager m_game;

    private CombatContext m_context;
    private TurnSystem m_turn;
    private ReactionSystem m_reaction;
    private ActionSystem m_action;
    private TransitionSystem m_transition;
    private DamageSystem m_damage;
    public ActionSystem Action => m_action;

    private CombatCommandDispatcher m_commandDispatcher;
    private CombatCommandProcessor m_commandProcessor;

    public event Action OnCombatStarted;
    public event Action<CombatResult> OnCombatEnded;
    public event Action<CombatActor> OnTurnStart;
    public event Action<CombatActor> OnTurnEnd;

    private CombatArea m_area;

    private List<CombatArea> m_areasInScene;

    private CombatSaveData m_save;

    public CombatManager(GameManager game)
    {
        m_game = game;
    }
    public CombatSaveData Save() 
    {
        return m_save;
    }
    public void Load(CombatSaveData data) 
    {
        m_save = data;

        foreach (var area in m_areasInScene) 
        {
            if (m_save != null) 
            {
                foreach (var completed in m_save.CompletedAreas)
                {
                    if (completed == area.ID)
                        area.SetCompleted(true);
                }
            }            
            area.Initialize(m_game);
        }
        SetReady();
    }
    #region IManager
    public override void Update(float dt)
    {
        if (m_state == CombatState.Inactive) return;

        m_reaction.Update(dt);
        m_transition.Update(dt);
    }
    public override void OnSceneLoaded(SceneData data) 
    {
        IsReady = false;

        m_areasInScene =
               GameObject.
                FindObjectsByType<CombatArea>
               (FindObjectsSortMode.None).
               ToList();
    }
    public override bool Dispose()
    {
        CleanupSystems();
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
        m_damage = new DamageSystem();

        m_turn = new TurnSystem(m_context);

        m_reaction = new ReactionSystem();
        m_action = new ActionSystem(m_context, m_reaction);
        m_action.OnActionFinished += ActionFinished;
        m_action.OnActionSubmitted += ActionSubmitted;
        m_action.OnActionResolved += m_damage.ActionResolved;

        m_transition = new TransitionSystem(area);
        m_transition.OnTransitionFinished += TransitionFinished;

        m_commandDispatcher = new CombatCommandDispatcher(m_action, m_reaction);
        m_commandProcessor = new CombatCommandProcessor(actors, m_commandDispatcher);
          
        NextTurn();
        
    }
    private void ActionSubmitted(ActionContext actx)
    {
        if (actx.Action == null) 
        {            
            ActionFinished(actx);
            return;
        }
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
        CleanupSystems();
        m_game.SetState(GameState.None);
        
        OnCombatEnded?.Invoke(result);
        ChangeState(CombatState.Inactive);
        CombatEvents.CombatEnded(result);

        if (result == CombatResult.Won) 
        {
            if (m_save == null) 
            {
                m_save = new CombatSaveData();
            }
            if (!m_save.CompletedAreas.Contains(m_area.ID)) 
            {
                m_save.CompletedAreas.Add(m_area.ID);
            }
            
        }

        m_area = null;
    }
    private void CleanupSystems() 
    {
        // add dispose systems
        
        if (m_action != null)
        {
            m_action.OnActionFinished -= ActionFinished;
            m_action.OnActionSubmitted -= ActionSubmitted;
            m_action.OnActionResolved -= m_damage.ActionResolved;
        }
        if (m_transition != null)
        {
            m_transition.OnTransitionFinished -= TransitionFinished;
        }
    }
    public void ChangeState(CombatState state) 
    {
        if (state == m_state) return;

        m_state = state;
        CombatEvents.CombatStateChanged(m_state);
    }
}
