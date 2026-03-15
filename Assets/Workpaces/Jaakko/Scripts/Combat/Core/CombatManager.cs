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
    private TurnSystem m_turn;
    private ReactionSystem m_reaction;    
    public ReactionSystem Reaction => m_reaction;
    private ActionSystem m_action;
    public ActionSystem Action => m_action;

    /*
    ==================================================================COMBAT MANAGER===============================================================    
        Flow:
            1. CombatArea: Player Enters CombatArea (see CombatArea.cs).
            3. CombatArea calls CombatManager.StartBattle with all participant Actors

            4. CombatManager: StartCombat() is called and all systems are created. 
            5. CombatManager: StartCombat(): CombatManager subscribes to ActionSystem.OnActionFinished.

            6. CombatManager: StartNextTurn() Gets the next Actor from TurnSystem
            7. CombatManager: Sets CombatContext.CurrentActor to the next actor
            8. CombatManager: Calls ActionSystem.TurnStart()                              

            9. ActionSystem: TurnStart() will notify CurrentActor that an action is requested.
           10. ActionSystem: CurrentActor will call ActionSystem.SubmitAction()
           11. ActionSystem: SubmitAction(): calls CombatAction.Resolve() which plays animation   

           12. CombatActor: calls ActionSystem.NotifyActionFinished()
           13. ActionSystem: Invokes OnActionFinished => CombatManager.ActionFinished

           14. CombatManager: ActionFinished(): Checks game end ? else StartNextTurn()


            Components:
            1. CombatContext just holds context for combat such as CurrentActor, Actors, TurnIndex ect.. Modules may modify the context
            2. TurnSystem advances turn based on CombatContext. Returns the NextPlayer in Actors after CurrentActor that is not dead
            3. ActionSystem Requests a CombatAction from The CurrentActor and waits for CombatAction submit call            
            4. ReactionSystem handles prompts and consumes prompts from the ReactionWindow
    
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
        CleanupSystems();
        return true;
    }
    #endregion
    public void StartCombat(List<CombatActor> actors)
    {
        if (m_state != CombatState.Inactive) return;

        CombatEvents.CombatActorsChanged(actors);

        m_state = CombatState.Active;        

        m_context = new CombatContext(actors);
        m_turn = new TurnSystem(m_context);
        m_reaction = new ReactionSystem();
        m_action = new ActionSystem(m_context, m_reaction);

        // this event is raised when the action animation is finished
        m_action.OnActionFinished += ActionFinished;

        NextTurn();
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
    private void NextTurn() 
    {
        CombatActor actor = m_turn.Next();
        if (actor == null) 
        {
            Debug.LogWarning($"Next Actor == NULL, ENDING");
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

        CombatResult result = m_context.Actors.ToList()
            .Exists(a => a.IsPlayer && !a.IsDead)
            ? CombatResult.Won : CombatResult.Lost;

        CombatEvents.CombatEnded(result);

        m_state = CombatState.Inactive;
        CleanupSystems();        
    }
    private void CleanupSystems() 
    {
        m_action.OnActionFinished -= ActionFinished;

        m_action = null;
        m_reaction = null;
        m_turn = null;
        m_context = null;
    }
}
