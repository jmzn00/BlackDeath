using System;
using System.Collections.Generic;

public static class CombatEvents
{
    // COMBAT STATE
    public static event Action OnCombatStarted;
    public static event Action<CombatResult> OnCombatEnded;
    public static event Action<CombatState> OnCombatStateChanged;

    // ACTORS
    public static event Action<List<CombatActor>> OnCombatActorsChanged;
    public static event Action<CombatActor> OnActorDied;

    // ACTION
    public static event Action<ActionContext> OnActionFinished;
    public static event Action<ActionContext> OnActionSubmitted;
    public static event Action<ActionContext, ActionResult> OnActionResolved;

    // REACTION
    public static event Action<ActionContext> OnReactionWindowOpened;
    public static event Action<ActionContext> OnReactionWindowClosed;
    public static event Action<ActionContext, ActionResult> OnReactionResolved;

    // TURN
    public static event Action<CombatActor> OnTurnStarted;
    public static event Action<CombatActor> OnTurnEnded;

    // TRANSITION
    public static event Action OnTransitionStarted;
    public static event Action OnTransitionEnded;

    public static void TransitionStarted() 
    {
        OnTransitionStarted?.Invoke();
    }
    public static void TransitionEnded() 
    {
        OnTransitionEnded?.Invoke();
    }

    public static void CombatStarted() 
    {
        OnCombatStarted?.Invoke();
    }
    public static void CombatEnded(CombatResult res) 
    {
        OnCombatEnded?.Invoke(res);
    }
    public static void TurnStarted(CombatActor actor) 
    {
        OnTurnStarted?.Invoke(actor);
    }
    public static void TurnEnded(CombatActor actor) 
    {
        OnTurnEnded?.Invoke(actor);
    }
    public static void ActionSubmitted(ActionContext ctx) 
    {
        OnActionSubmitted?.Invoke(ctx);
    }
    public static void ActionResolved(ActionContext ctx, ActionResult res) 
    {
        OnActionResolved?.Invoke(ctx, res);
    }
    public static void ActorDied(CombatActor actor) 
    {
        OnActorDied?.Invoke(actor);
    }
    public static void ReactionWindowOpened(ActionContext ctx) 
    {
        OnReactionWindowOpened?.Invoke(ctx);
    }
    public static void ReactionWindowClosed(ActionContext ctx) 
    {
        OnReactionWindowClosed?.Invoke(ctx);
    }
    public static void ReactionResolved(ActionContext ctx, ActionResult res) 
    {
        OnReactionResolved?.Invoke(ctx, res);
    }
    public static void ActionFinished(ActionContext ctx) 
    {
        OnActionFinished?.Invoke(ctx);
    }
    public static void CombatActorsChanged(List<CombatActor> actors) 
    {
        OnCombatActorsChanged?.Invoke(actors);
    }
    public static void CombatStateChanged(CombatState state) 
    {
        OnCombatStateChanged?.Invoke(state);
    }
}
