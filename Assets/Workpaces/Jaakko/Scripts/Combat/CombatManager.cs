using JetBrains.Annotations;
using Mono.Cecil;
using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ActionContext
{
    public ActionContext()
    {

    }
    public CombatActor Source;
    public CombatActor Target;

    public CombatAction Action;

    public string PromptKey;
    public InputPrompt Prompt;
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
    ActorTurn,
    ResolvingAction,
    Ending
}
public class CombatManager : IManager
{
    private CombatState m_state = CombatState.None;
    public CombatState State => m_state;

    private bool m_waitingForResolve;

    private InputManager m_input;
    private ActorManager m_actorManager;
    private GameManager m_game;
    private UIManager m_uiManager;

    private CombatArea m_currentArea;
    private int m_turnIndex;

    private List<CombatActor> m_partyCombatActors;
    private List<CombatArea> m_combatAreas = new List<CombatArea>();
    private List<CombatActor> m_combatActors = new List<CombatActor>();

    private Actor m_actor;
    public Actor Actor => m_actor;
    private CombatActor m_currentActor;
        
    private ReactiveWindow m_reactiveWindow;
    public ReactiveWindow ReactiveWindow => m_reactiveWindow;

    public event Action<Actor> OnCurrentActorChanged;
    public Action<CombatContext> OnContextChanged;
    public event Action<bool> OnCombatStarted;
    private event Action<ActionContext, ActionResult> OnActionResolved;

    public CombatManager(GameManager game)
    {
        m_game = game;

        m_input = m_game.Resolve<InputManager>();
        m_actorManager = m_game.Resolve<ActorManager>();
        m_uiManager = m_game.Resolve<UIManager>();        
    }
    #region IManager
    public bool Init()
    {
        m_combatAreas = new List<CombatArea>(
            GameObject.FindObjectsByType<CombatArea>
            (FindObjectsSortMode.None).ToList());

        foreach (var c in m_combatAreas)
            c.Initialize(this);

        m_reactiveWindow = new ReactiveWindow();
        m_reactiveWindow.OnWindowClosed += ResolveAction;
        return true;
    }
    public void OnManagersInitialzied() 
    {
        
    }
    public bool Dispose()
    {
        m_reactiveWindow.OnWindowClosed -= ResolveAction;
        return true;
    }
    #endregion
    public void Update(float dt)
    {
        if (m_state == CombatState.None) return;
        m_reactiveWindow.Update(dt);

        switch (m_state)
        {
            case CombatState.Starting:
                BeginFirstTurn();
                break;
            case CombatState.ActorTurn:
                ProcessTurn();
                break;
            case CombatState.ResolvingAction:
                if (!m_waitingForResolve)
                    AdvanceTurn();
                break;
            case CombatState.Ending:
                EndBattle();
                break;
        }
    }
    public void OnActorDied(CombatActor actor)
    {
        if (!m_combatActors.Contains(actor)) return;

        if (actor == m_currentActor)
        {
            AdvanceTurn();
        }
        UpdateContext();
    }
    private void UpdateContext()
    {
        CombatContext context = new CombatContext()
        {
            Actors = m_combatActors,
            CurrentActor = m_currentActor,
            TurnIndex = m_turnIndex
        };
        OnContextChanged?.Invoke(context);

        foreach (var a in m_combatActors)
            a.OnCombatContextChanged(context);
    }

    private void AdvanceTurn()
    {
        if (m_combatActors == null || m_combatActors.Count == 0)
        {
            m_state = CombatState.Ending;
            return;
        }
        m_currentActor.OnTurnEnd();
        int attempts = 0;
        do
        {
            m_turnIndex = (m_turnIndex + 1) % m_combatActors.Count;
            m_currentActor = m_combatActors[m_turnIndex];
            attempts++;
        } while (m_currentActor.IsDead && attempts < m_combatActors.Count);

        if (CheckBattleEnd())
        {
            m_state = CombatState.Ending;
            return;
        }
        OnCurrentActorChanged?.Invoke(m_currentActor.Actor);
        UpdateContext();
        m_currentActor.OnTurnStart();
        m_state = CombatState.ActorTurn;
    }
    private void ProcessTurn()
    {
        if (CheckBattleEnd()) 
        {
            EndBattle();
        }
        if (m_currentActor == null || m_currentActor.IsDead)
        {
            AdvanceTurn();
            return;
        }
        if (m_currentActor.IsPlayer)
            m_uiManager.SetInputMode(UIInputMode.Navigation);

        m_currentActor.
            ActionProvider.
            RequestAction(m_currentActor, m_combatActors);
    }
    public void SubmitAction(ActionContext ctx)
    {
        if (m_state == CombatState.ResolvingAction)
        {
            Debug.LogWarning("Action blocked: already resolving another action");
            return;
        }
        if (ctx == null || ctx.Action == null)
        {
            Debug.Log("Context Or Action Is NULL");
            return;
        }
        if (ctx.Source != m_currentActor)
        {
            return;
        }
        if (ctx.Target == null)
        {
            Debug.Log("Context Target is NULL");
            return;
        }
        m_uiManager.SetInputMode(UIInputMode.Combat);
        ExecuteAction(ctx);
    }
    private void ExecuteAction(ActionContext ctx)
    {
        m_state = CombatState.ResolvingAction;
        m_waitingForResolve = true;

        ctx.Action.Resolve(ctx, () =>
        {
            m_waitingForResolve = false;
        });
    }    
    private void ResolveAction(ActionContext ctx)
    {
        if (ctx == null || ctx.Action == null)
        {
            Debug.LogWarning("Cannot Resolve Action. Ctx is NULL");
            return;
        }

        ReactionType attackerReaction = m_reactiveWindow.ConsumeAttackerReaction();
        ReactionType defenderReaction = m_reactiveWindow.ConsumeDefenderReaction();

        ActionResult result = ActionResult.Hit;        
        switch (defenderReaction) 
        {
            case ReactionType.Parry:
                result = ActionResult.Parried;
                break;
            case ReactionType.Dodge:
                result = ActionResult.Dodged;
                break;
        }
        if (attackerReaction == ReactionType.Confirm && result == ActionResult.Hit) 
        {
            result = ActionResult.Confirmed;
        }

        Debug.Log($"Action Result: {result}");

        ctx.Action.ResolveResult(ctx, result);
        OnActionResolved?.Invoke(ctx, result);
    }

    public void StartBattle(CombatPreferences prefs)
    {
        if (m_state != CombatState.None)
            return;
        m_currentArea = prefs.m_area;
        m_input.ToggleInput(false);

        m_uiManager.SetInputMode(UIInputMode.Combat);        
        
        List<CombatActor> participants = new List<CombatActor>();
        int spawns = 0;
        foreach (var t in prefs.m_enemySpawnPoints)
        {
            Actor a =
                GameObject.Instantiate(prefs.m_enemies[0], t.position, t.rotation);
            a.gameObject.name += spawns.ToString();
            spawns++;

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
            }
        }

        m_combatActors = participants;
        m_turnIndex = 0;

        m_state = CombatState.Starting;
        OnCombatStarted?.Invoke(true);
    }
    private void BeginFirstTurn()
    {
        m_game.SetState(GameState.Combat);

        m_currentActor = m_combatActors[m_turnIndex];
        m_actor = m_currentActor.Actor;
        OnCurrentActorChanged?.Invoke(m_actor);

        m_state = CombatState.ActorTurn;

        foreach (var a in m_combatActors)
            a.OnCombatStarted();
        UpdateContext();
    }
    private bool CheckBattleEnd()
    {
        bool alliesAlive = m_combatActors.Exists(c => c.IsPlayer && !c.IsDead);

        bool enemiesAlive = m_combatActors.Exists(c => !c.IsPlayer && !c.IsDead);
        return !enemiesAlive || !alliesAlive;
    }
    private void EndBattle()
    {
        foreach (var a in m_combatActors)
            a.OnCombatFinished();

        m_input.ToggleInput(true);
        m_state = CombatState.None;
        m_game.SetState(GameState.None);

        // TEMP. Set based on win / loss
        m_currentArea.SetCompleted(true);

        m_combatActors.Clear();
        m_partyCombatActors.Clear();

        m_uiManager.SetInputMode(UIInputMode.None);
        OnCombatStarted?.Invoke(false);
    }
}
