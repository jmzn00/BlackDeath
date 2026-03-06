using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class ActionContext 
{
    public ActionContext() 
    {
    
    }
    public CombatActor Source;
    public CombatActor Target;

    public CombatAction Action;
}
public class CombatContext
{
    public CombatContext()
    {

    }
    public List<CombatActor> Actors;
    public CombatActor CurrentActor;
    public int TurnIndex;
}
public enum CombatState 
{
    None,
    Starting,
    PlayerTurn,
    EnemyTurn,
    ResolvingAction,
    Ending
}
public class CombatManager : IManager
{
    private List<CombatArea> m_combatAreas = new List<CombatArea>();

    private InputManager m_input;

    private List<CombatActor> m_combatActors = new List<CombatActor>();
    private int m_turnIndex;

    private CombatState m_state = CombatState.None;
    public CombatState State => m_state;

    private CombatActor m_currentActor;
    private CombatActor m_playerActor;

    private ReactiveWindow m_reactiveWindow = new ReactiveWindow();

    private bool m_waitingForResolve;
    private float m_actionTimeout;
    private const float ACTION_TIMEOUT = 3f;

    private ActorManager m_actorManager;
    private GameManager m_game;

    public event Action<Actor> OnCurrentActorChanged;
    private Actor m_actor;
    public Actor Actor => m_actor;

    public CombatManager(InputManager input, ActorManager actorManager, GameManager game)
    {
        m_input = input;
        m_game = game;
        m_actorManager = actorManager;
    }

    public bool Init()
    {
        m_combatAreas = new List<CombatArea>(
            GameObject.FindObjectsByType<CombatArea>
            (FindObjectsSortMode.None).ToList());

        foreach (var c in m_combatAreas)
            c.Initialize(this);

        return true;
    }
    public void OnManagersInitialzied()
    {

    }
    public bool Dispose()
    {
        return true;
    }
    public void Update(float dt)
    {
        if (m_state == CombatState.None) return;

        m_reactiveWindow.Update(dt);

        HandleDefensiveInput();

        switch (m_state) 
        {
            case CombatState.Starting:
                BeginFirstTurn();
                break;
            case CombatState.PlayerTurn:
            case CombatState.EnemyTurn:
                ProcessTurn();
                break;
            case CombatState.ResolvingAction:
                UpdateResolve(dt);
                break;
            case CombatState.Ending:
                EndBattle();
                break;
        }
    }
    List<CombatActor> m_partyCombatActors;
    List<CombatActor> m_enemyCombatActors;
    public void StartBattle(CombatPreferences prefs)
    {
        if (m_state != CombatState.None)
            return;        
        m_input.ToggleInput(false);
        List<CombatActor> participants = new List<CombatActor>();
        foreach (var t in prefs.m_enemySpawnPoints) 
        {
            Actor a = 
                GameObject.Instantiate(prefs.m_enemies[0], t.position, t.rotation);

            if (a == null) 
            {
                Debug.Log("Actor Component is NULL");
                continue;
            }
            else 
            {
                a.Init(m_game);
                CombatActor ca = a.Get<CombatActor>();
                if (ca == null) 
                {
                    Debug.LogWarning($"CombatActor NULL on {a.name}");
                    continue;
                }
                participants.Add(ca);
            }
        }
        List<Actor> partyActors = m_actorManager.Party.ToList();
        int spawnIndex = 0;
        m_partyCombatActors = new List<CombatActor>();
        
        foreach (Actor pa in partyActors) 
        {
            CombatActor ca = pa.Get<CombatActor>();
            if (ca == null) 
            {
                Debug.Log("Combat Actor NULL on party memeber");
                continue;
            }
            else 
            {
                participants.Add(ca);
                m_partyCombatActors.Add(ca);
                pa.Get<MovementController>().Move(prefs.m_partySpawnPoints[spawnIndex].position);
                spawnIndex++;
                Debug.Log($"Combat Participant Added {ca.name}");
            }                
        }        
        
        m_combatActors = participants;        
        m_turnIndex = 0;
        m_playerActor = participants.Find(p => p.IsPlayer);
        
        m_state = CombatState.Starting;        
    }
    private void BeginFirstTurn() 
    {
        m_game.SetState(GameState.Combat);
        m_state = CombatState.PlayerTurn;

        m_currentActor = m_combatActors[m_turnIndex];
        m_actor = m_currentActor.Actor;
        OnCurrentActorChanged?.Invoke(m_actor);        

        foreach (var a in m_combatActors)
            a.OnCombatStarted();
        UpdateContext();
    }
    void UpdateContext() 
    {
        CombatContext context = new CombatContext()
        {
            Actors = m_combatActors,
            CurrentActor = m_currentActor,
            TurnIndex = m_turnIndex
        };
        foreach (var a in m_combatActors)
            a.OnCombatContextChanged(context);
    }
    private void ProcessTurn()
    {        
        if (m_currentActor == null || m_currentActor.IsDead)
        {
            AdvanceTurn();
            return;
        }
        if (m_currentActor.IsPlayer)
        {
            // Get Action
            // Get Target
            // Execute
            // TODO 
            //m_currentActor.RequestAction(m_combatActors);
            ActionContext ctx = m_currentActor.RequestAction(m_combatActors);

            if (ctx == null)
            {
                return;
            }
            else
            {
                ExecuteAction(ctx);   
            }

        }
        else
        {
            //ExecuteAction(m_currentActor, m_playerActor);
        }        
    }
    private void ExecuteAction(ActionContext ctx) 
    {
        /*
        if (ctx.Source == null || ctx.Target == null) 
        {
            return;
        } */
        m_state = CombatState.ResolvingAction;
        m_waitingForResolve = true;
        m_actionTimeout = ACTION_TIMEOUT;
        
    }
    private void UpdateResolve(float dt) 
    {
        if (!m_waitingForResolve) 
        {
            AdvanceTurn();
            return;
        }

        m_actionTimeout -= dt;

        if (m_actionTimeout <= 0f) 
        {
            ResolveAction();
        }
    }
    private bool CheckBattleEnd() 
    {
        if (m_playerActor == null || m_playerActor.IsDead)
            return true;

        bool enemiesAlive = m_combatActors.Exists(c => !c.IsPlayer && !c.IsDead);
        return !enemiesAlive;
    }
    public void NextTurn() 
    {
        if (m_state == CombatState.None) return;

        AdvanceTurn();
    }
    private void AdvanceTurn() 
    {
        if (CheckBattleEnd()) 
        {
            m_state = CombatState.Ending;
            return;
        }
        m_turnIndex = (m_turnIndex + 1) % m_combatActors.Count;
        m_currentActor = m_combatActors[m_turnIndex];
        if (m_currentActor.IsPlayer)
        {
            m_actorManager.SetControlledActor(m_currentActor.GetComponent<Actor>());
        }
        
        OnCurrentActorChanged?.Invoke(m_currentActor.Actor);
        UpdateContext();

        m_state = m_currentActor.IsPlayer
            ? CombatState.PlayerTurn
            : CombatState.EnemyTurn;
    }
    private void EndBattle()
    {
        foreach (var a in m_combatActors)
            a.OnCombatFinished();

        m_input.ToggleInput(true);
        m_state = CombatState.None;
        m_game.SetState(GameState.None);

        m_combatActors.Clear();
        m_partyCombatActors.Clear();        
    }
    private void HandleDefensiveInput() 
    {
        if (m_state != CombatState.EnemyTurn)
            return;
        InputState state = m_input.GetInputState();
        if (state.ParryPressed) 
        {
            m_reactiveWindow.TryActivateParry();
        }
        if (state.DodgePressed) 
        {
            m_reactiveWindow.TryActivateDodge();
        }
    }

    // ================
    //     ANIMATION
    // ================
    public void OnWindowClose(CombatActor attacker, CombatActor target) 
    {
        bool parried = m_reactiveWindow.ParryActive;
        bool dodged = m_reactiveWindow.DodgeActive && !parried;

        if (!dodged)
            Debug.Log($"{target} took damage");
        if (parried)
            Debug.Log($"{target} parried");
        
        m_reactiveWindow.Reset();
    }
    public void ResolveAction() 
    {
        m_waitingForResolve = false;
        m_state = CombatState.PlayerTurn;
    }
}