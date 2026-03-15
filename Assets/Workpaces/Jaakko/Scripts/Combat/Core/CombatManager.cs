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

    /*
    ==================================================================COMBAT MANAGER===============================================================

        Components:
            1. CombatContext just holds context for combat such as CurrentPlayer, Actors, TurnIndex ect.. Modules may modify the context
            2. TurnSystem advances turn based on CombatContext. Returns the NextPlayer in Actors after CurrentActor that is not dead
            3. ActionSystem Requests an action from The CurrentPlayer and waits for submit after which it invokes the event that action is finished,
               Note that ActionSystem also Closes And Opens the ReactionSystem for now but it will be moved to ReactionSystem
            4. ReactionSystem handles prompts and consumes prompts from the ReactionWindow, Note that ReactionSystem owns ReactionWindow
            
        Flow:
            1. CombatArea: Player Enters CombatArea (see CombatArea.cs). CombatArea calls CombatManager.StartBattle with all participants

            2. CombatManager: StartCombat() is called and all systems are created. CombatManager subscribes to ActionSystem.OnActionFinished.
                              This is because the CombatManager will resolve the final result when the action is finished

            3. CombatManager: StartNextTurn() Gets the next Actor from TurnSystem and sets CombatContext.CurrentPlayer to Actor.    
                              After this ActionSystem.TurnStart is called.

            4. ActionSystem: TurnStart() will notify the Actor that an action is requested. Once the Actor submits an action
                             ActionSystem.SubmitAction is Called. It will then resolve the action which plays the animation
                             and opens reacion windows ect..

            5. ActionSystem: NotifyActionFinished() is called by the actor whos action was being resolved. This invokes the OnActionFinished
                             Event that the CombatManager listens to.

            6. CombatManager: Once OnActionFinished is invoked CombatManager.ActionFinished() is called which will resolve the input results
                              from ReactionSystem and then calls Action.ResolveResults(). ResolveResults() is in CombatAction and decides what
                              should be done based on the result ie. hit, dodge, parry, confirm

            7. CombatManager: Once Action.ResolveResults() has been called and damage applied ect.. then CombatManager checks if we should end
                              combat ie. all Players or Enemies dead. If not then call CombatManager.StartNextTurn();   
    
        Other:
            1. CombatEvents: If you see this you dont need to pay attention to it. It is not used for any combat logic only so external things
                             like UI or CameraManager can know what is happening in combat

    ================================================================================================================================================
     */
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

        // drive camera immediately for the upcoming turn/transition
        var cam = m_game.Resolve<CameraManager>();
        if (cam != null)
        {
            // uses preset ids you created in the director
            string presetId = actor.IsPlayer ? "CC_PlayerTurn" : "CC_EnemyPrepare";
            cam.TransitionToPreset(presetId, actor, null);
        }

        // Transition window
        var transitionView = GameObject.FindFirstObjectByType<TurnTransitionView>();
        if (transitionView != null)
        {
            m_state = CombatState.NextTurn;
            transitionView.PlayTransition(actor, 4f, () =>
            {
                CombatEvents.TurnStarted(actor);
                m_context.SetCurrentActor(actor);
                m_context.AdvanceTurn();
                m_action.TurnStarted();

                m_state = CombatState.Active;
            });
            return;
        }

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
