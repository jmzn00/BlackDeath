using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;
public enum CombatUIState 
{
    ActionTypeSelecting,
    ActionSelecting,
    TargetSelecting,
    None
}
public class CombatUI : UIComponentBase<CombatUIViewGroup>
{
    private InputManager m_input;
    private CombatManager m_combat;

    private ActionView m_actionView;
    private TargetView m_targetView;
    private DamageView m_damageView;
    private ReactionView m_reactionView;
    private StatusView m_statusView;
    private ResultView m_resultView;

    private CombatActor m_currentActor;

    private CombatAction m_currentAction;
    private CombatActor m_currentTarget;

    private List<CombatActor> m_currentParticipants;
    private int m_currentTargetIndex = 0;

    private CombatUIState m_state;

    public CombatUI(GameManager game, CombatUIViewGroup group)
        : base(game, group)
    {
        m_input = game.Resolve<InputManager>();
        m_combat = game.Resolve<CombatManager>();

        m_actionView = group.Get<ActionView>();
        m_targetView = group.Get<TargetView>();
        m_damageView = group.Get<DamageView>();
        m_reactionView = group.Get<ReactionView>();
        m_statusView = group.Get<StatusView>();
        m_resultView = group.Get<ResultView>();  
    }
    public override void Initialize()
    {
        base.Initialize();

        CombatEvents.OnTurnStarted += TurnStart;
        CombatEvents.OnTurnEnded += TurnEnd;
        CombatEvents.OnCombatActorsChanged += ActorsChanged;

        CombatEvents.OnAttackerPromptOpened += AttackerPromptOpened;
        CombatEvents.OnReactionWindowClosed += ReactionWindowClosed;

        CombatEvents.OnActorDied += ActorDied;

        CombatEvents.OnCombatStarted += CombatStarted;
        CombatEvents.OnCombatEnded += CombatEnded;

        m_input.OnSelectTarget += SelectTarget;        
    }
    public override void Dispose()
    {
        base.Dispose();

        m_input.OnSelectTarget -= SelectTarget;

        CombatEvents.OnTurnStarted -= TurnStart;
        CombatEvents.OnTurnEnded -= TurnEnd;
        CombatEvents.OnCombatActorsChanged -= ActorsChanged;
        CombatEvents.OnActorDied -= ActorDied;

        CombatEvents.OnCombatStarted -= CombatStarted;
        CombatEvents.OnCombatEnded -= CombatEnded;

        List<UIViewBase> views = m_group.GetAllViews();
    }
    private void CombatStarted() 
    {
        m_statusView.View();
    }
    private void CombatEnded(CombatResult res) 
    {
        m_statusView.Hide();

        m_resultView.DisplayResults(m_combat.Container
            .Resolve<CombatStatSystem>().GetStatsOrdered(),
            res);

        m_resultView.View();
    }
    private void AttackerPromptOpened(InputPrompt prompt) 
    {
        m_reactionView.AttackerPromptOpened(prompt);
        m_reactionView.View();
    }
    private void ReactionWindowClosed(ActionContext ctx) 
    {
        m_reactionView.Hide();
    }
    public override bool OnSubmit()
    {
        if (m_resultView.gameObject.activeInHierarchy) 
        {
            m_resultView.Hide();
            m_combat.EndScreenFinished();
            m_resultView.ClearPortraits();
            return true;
        }

        switch (m_state)
        {
            case CombatUIState.ActionTypeSelecting:
            case CombatUIState.ActionSelecting:
                return true;
            case CombatUIState.TargetSelecting:
                SubmitAction();
                return true;
        }        
        return false;
    }
    public override bool OnCancel()
    {
        GoBack();
        return true;
    }
    private void ActorsChanged(List<CombatActor> actors)
    {
        m_currentParticipants = new List<CombatActor>(actors);
        m_statusView.ActorsChanged(m_currentParticipants);
    }
    private void ActorDied(CombatActor actor) 
    {
        m_statusView.ActorDied(actor);
    }
    private void SelectTarget(float value)
    {
        if (m_game.State != GameState.Combat) return;

        if (m_currentParticipants == null)
        {
            Debug.LogWarning($"Cannot Select Target, participants is NULL");
            return;
        }
        if (m_currentAction == null)
        {
            Debug.LogWarning($"Cannot Select Target, Action is NULL");
            return;
        }
        List<CombatActor> validTargets
            = m_currentAction.GetValidTargets(m_currentActor,
            m_currentParticipants);

        if (validTargets.Count == 0)
        {
            Debug.LogWarning($"CombatUI: No valid Targets");
            return;
        }

        if (value > 0)
        {
            m_currentTargetIndex++;
            if (m_currentTargetIndex >= validTargets.Count)
                m_currentTargetIndex = 0;
        }
        else if (value < 0)
        {
            m_currentTargetIndex--;
            if (m_currentTargetIndex < 0)
                m_currentTargetIndex = validTargets.Count - 1;
        }

        m_currentTarget = validTargets[m_currentTargetIndex];
        m_targetView.ChangeTarget(m_currentTarget);
        m_targetView.SetPosition(m_currentTarget.transform.position);

        CameraAnimationEvents.NotifyTargetChanged(m_currentTarget.Target);
        CombatEvents.ActorTargetChanged(m_currentActor, m_currentTarget);
    }
    private void SubmitAction()
    {
        if (m_currentActor == null
            || m_currentAction == null) return;

        ActionContext ctx = new ActionContext
        {
            Source = m_currentActor,
            Action = m_currentAction
        };

        switch (m_currentAction.targetType) 
        {
            case TargetType.Self:
                ctx.Target = m_currentActor;
                break;
            case TargetType.Enemy:
            case TargetType.Ally:
            case TargetType.AOEEnemy:
            case TargetType.AOEAlly:
                if (m_currentTarget == null) return;
                ctx.Target = m_currentTarget;

                ctx.Targets = m_currentAction.GetValidTargets(
                    m_currentActor,
                    m_currentParticipants).ToArray();
                break;
        }
        m_currentActor.
            ActionProvider.SetAction(ctx);
        m_targetView.Hide();
    }
    private void GoBack()
    {
        if (m_currentActor == null) return;

        switch (m_state)
        {
            case CombatUIState.ActionTypeSelecting:

                break;
            case CombatUIState.ActionSelecting:
                m_actionView.ShowTypeButtons(m_currentActor.Actions);
                break;
            case CombatUIState.TargetSelecting:
                
                break;
        }
    }
    private void TurnStart(CombatActor actor)
    {
        if (actor.Team == Team.Enemy) return;
        
        
        m_currentActor = actor;
        m_currentAction = null;
        m_currentTarget = null;

        m_actionView.View();
        m_actionView.SetPosition(actor.transform.position);

        m_actionView.OnActionTypeSelected += ActionTypeSelected;
        m_actionView.OnActionSelected += ActionSelected;

        m_actionView.ShowTypeButtons(actor.Actions);

        // for camera
        CombatEvents.ActorStateChanged(m_currentActor, CombatActorState.ActionSelecting);
        m_state = CombatUIState.ActionTypeSelecting;
        
    }
    private void TurnEnd(CombatActor actor)
    {
        if (actor.Team == Team.Enemy) return;

        m_currentAction = null;
        m_currentTarget = null;
        m_currentActor = null;

        m_actionView.Hide();
        m_targetView.Hide();

        m_actionView.OnActionTypeSelected -= ActionTypeSelected;
        m_actionView.OnActionSelected -= ActionSelected;
    }
    private void ActionTypeSelected(Type type)
    {
        m_state = CombatUIState.ActionSelecting;
        m_actionView.ShowActionsOfType(type, m_currentActor.Actions);
    }
    private void ActionSelected(CombatAction action)
    {
        if (!action.CanExecute(m_currentActor,
            out string reason))
        {
            // display this in ui later. ScreenSpace?
            Debug.Log($"{action.actionName} cannot be performed: {reason}");
            return;
        }

        // jank fix for action button submit / submit action in the same frame
        m_ui.ConsumeSubmit();

        m_actionView.Hide();
        m_currentAction = action;

        switch (action.targetType)
        {
            case TargetType.AOEAlly:
            case TargetType.Self:
                // skip target selection
                SubmitAction();
                break;
            case TargetType.AOEEnemy:
            case TargetType.Enemy:
            case TargetType.Ally:
                m_targetView.View();
                CombatEvents.ActorStateChanged(m_currentActor, CombatActorState.Targeting);
                m_state = CombatUIState.TargetSelecting;
                m_currentTargetIndex = 0;
                SelectTarget(0);
                break;
        }
    }
    public override void Toggle(bool show)
    {
        base.Toggle(show);

        if (show)
        {
            m_actionView.View();
        }
        else
        {
            m_group.HideAll();
        }
    }
    public override bool IsVisible()
    {
        return m_actionView.gameObject.activeInHierarchy;
    }
}
