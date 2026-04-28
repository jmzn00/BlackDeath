using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;
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

    private GameManager m_game;

    private CombatContext m_context;

    private CombatArea m_area;

    private List<CombatArea> m_areasInScene;

    private CombatSaveData m_save;

    private List<CombatSystemBase> m_systems = new();

    private Container m_container;
    public Container Container => m_container;

    public CombatManager(GameManager game)
    {
        m_game = game;
    }
    #region IManager
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
                    {
                        area.SetCompleted(true);
                    }
                    
                }
            }
            area.Initialize(m_game);
        }
        SetReady();
    }
    public override void Update(float dt)
    {
        if (m_state == CombatState.Inactive) return;

        for (int i = 0; i < m_systems.Count; i++)
            m_systems[i].Update(dt);
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
    public override bool Init()
    {
        m_container = new Container();

        m_container.RegisterInstance<CombatManager>(this);

        m_container.Register<TurnSystem>();
        m_container.Register<ReactionSystem>(); 
        m_container.Register<ActionSystem>();
        m_container.Register<TransitionSystem>();
        m_container.Register<DamageSystem>();
        m_container.Register<CombatStatSystem>();

        m_container.Register<CombatCommandDispatcher>();
        m_container.Register<CombatCommandProcessor>();

        m_systems = m_container.GetAll<CombatSystemBase>().ToList();

        m_container.Resolve<ActionSystem>().OnActionFinished += ActionFinished;
        return true;
    }
    public override bool Dispose()
    {
        m_container.Resolve<ActionSystem>().OnActionFinished -= ActionFinished;

        foreach (var s in m_systems)
            s.Dispose();
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

        m_container.Resolve<TransitionSystem>().UpdateArea(area);

        m_context = new CombatContext(actors);
        foreach (var s in m_systems)
            s.Init(m_context);
          
        NextTurn();
    }
    private void NextTurn()
    {
        CombatActor actor = m_container.Resolve<TurnSystem>().Next();

        if (actor == null)
        {
            Debug.LogWarning($"TurnSystem.Next() == NULL");
            EndCombat();
            return;
        }
        CombatEvents.TurnStarted(actor);

        m_context.SetCurrentActor(actor);
        m_context.AdvanceTurn();

        m_container.Resolve<ActionSystem>().TurnStarted();
    }
    private void ActionFinished(ActionContext aCtx)
    {
        if (CheckEnd())
        {
            EndCombat();
        }
        else
        {
            NextTurn();
        }
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
        CombatResult result = CombatResult.Lost;
        if (m_context.Actors.ToList().Exists(a => a.Team == Team.Player
        && !a.IsDead)) 
        {
            result = CombatResult.Won;
        }
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

        foreach (var a in m_context.Actors)
            a.CombatEnded(result);

        foreach (var s in m_systems)
            s.Reset();

        m_area.AreaFinished(result);
        m_area = null;
        CombatEvents.CombatCameraEnded();
    }
    public void EndScreenFinished() 
    {
        m_game.SetState(GameState.None);
        ChangeState(CombatState.Inactive);
    }
    public void ChangeState(CombatState state) 
    {
        if (state == m_state) return;

        m_state = state;
        CombatEvents.CombatStateChanged(m_state);
    }
}
