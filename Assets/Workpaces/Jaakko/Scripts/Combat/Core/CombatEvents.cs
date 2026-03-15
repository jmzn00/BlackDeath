using System;
using System.Collections.Generic;

public static class CombatEvents
{
    // COMBATEVENTS IS NOT USED FOR COMBATSYSTEM
    // DO NOT USE COMBAT EVENTS FOR COMBAT LOGIC

    public static event Action OnCombatStarted;
    public static event Action<CombatResult> OnCombatEnded;

    public static event Action<CombatActor> OnTurnStarted;
    public static event Action<CombatActor> OnTurnEnded;

    public static event Action<ActionContext> OnActionSubmitted;
    public static event Action<ActionContext, ActionResult> OnActionResolved;

    public static event Action<CombatActor> OnActorDied;

    public static event Action<ActionContext> OnReactionWindowOpened;
    public static event Action<ActionContext> OnReactionWindowClosed;

    public static event Action<ActionContext, ActionResult> OnReactionResolved;
    
    public static event Action<ActionContext> OnActionFinished;
    public static event Action<List<CombatActor>> OnCombatActorsChanged;

    // New events for camera / UI integration (previewing target selection and action execution)
    // Note: These are for external systems (camera, UI). Combat logic should not depend on them.
    public static event Action<CombatActor, CombatActor> OnTargetSelected;
    public static event Action<CombatActor, CombatActor, CombatAction> OnActionExecuting;

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

    // New invokers
    public static void TargetSelected(CombatActor source, CombatActor target)
    {
        OnTargetSelected?.Invoke(source, target);
    }

    public static void ActionExecuting(CombatActor source, CombatActor target, CombatAction action)
    {
        OnActionExecuting?.Invoke(source, target, action);
    }
}
