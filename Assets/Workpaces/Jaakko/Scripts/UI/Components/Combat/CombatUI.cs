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
    private UIManager m_ui;

    private ActionView m_actionView;
    private TargetView m_targetView;
    private DamageView m_damageView;
    private ReactionView m_reactionView;

    private List<Button> m_buttons = new();

    private CombatActor m_currentActor;

    private CombatAction m_currentAction;
    private CombatActor m_currentTarget;

    private List<CombatActor> m_currentParticipants;
    private int m_currentTargetIndex = 0;

    private CombatUIState m_state;

    public CombatUI(GameManager game, CombatUIViewGroup group)
        : base(game, group)
    {
        m_actionView = group.ActionView;
        m_targetView = group.TargetView;
        m_damageView = group.DamageView;
        m_reactionView = group.ReactionView;

        m_targetView.Hide();
        m_actionView.Hide();
        m_damageView.Hide();
        m_reactionView.Hide();

        m_input = game.Resolve<InputManager>();
        m_ui = game.Resolve<UIManager>();
    }
    public override void Initialize()
    {
        m_actionView.OnButtonCreated += ButtonCreated;
        m_actionView.OnButtonRemoved += ButtonRemoved;

        CombatEvents.OnTurnStarted += TurnStart;
        CombatEvents.OnTurnEnded += TurnEnd;
        CombatEvents.OnCombatActorsChanged += ActorsChanged;

        CombatEvents.OnAttackerPromptOpened += AttackerPromptOpened;
        CombatEvents.OnReactionWindowClosed += ReactionWindowClosed;

        m_actionView.Init();
        m_targetView.Init();
        m_damageView.Init();

        m_input.OnSelectTarget += SelectTarget;
    }
    public override void Dispose()
    {
        m_actionView.OnButtonCreated -= ButtonCreated;
        m_actionView.OnButtonRemoved -= ButtonRemoved;

        m_input.OnSelectTarget -= SelectTarget;

        CombatEvents.OnTurnStarted -= TurnStart;
        CombatEvents.OnTurnEnded -= TurnEnd;
        CombatEvents.OnCombatActorsChanged -= ActorsChanged;
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

        CombatEvents.ActorTargetChanged(m_currentActor, m_currentTarget);
    }
    private void SubmitAction(CombatActor target = null)
    {
        if (m_currentActor == null
            || m_currentAction == null) return;
        if (target != null)
        {
            m_currentTarget = target;
        }
        if (m_currentTarget == null) return;

        ActionContext ctx = new ActionContext
        {
            Source = m_currentActor,
            Target = m_currentTarget,
            Action = m_currentAction
        };
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
                m_actionView.ClearActions();
                m_actionView.ShowActionTypes(m_currentActor.Actions);
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
        m_actionView.ShowActionTypes(actor.Actions);
        m_actionView.SetPosition(actor.transform.position);

        m_actionView.OnActionTypeSelected += ActionTypeSelected;
        m_actionView.OnActionSelected += ActionSelected;

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
        m_actionView.ShowActionsOfType(type, m_currentActor.Actions);
        m_state = CombatUIState.ActionSelecting;
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
            case TargetType.Self:
                SubmitAction(m_currentActor);
                break;
            case TargetType.Enemy:
            case TargetType.Ally:
                m_targetView.View();
                CombatEvents.ActorStateChanged(m_currentActor, CombatActorState.Targeting);
                m_state = CombatUIState.TargetSelecting;
                SelectTarget(0);
                break;
        }
    }
    private void ButtonCreated(Button button)
    {
        if (m_buttons.Contains(button))
        {
            Debug.LogWarning($"CombatUI: Trying to add a button that already exists");
            return;
        }
        m_buttons.Add(button);

        if (m_buttons.Count > 0)
        {
            m_ui.Navigation.UpdateButtons(m_buttons, m_buttons[0].gameObject);
        }
        else
        {
            m_ui.Navigation.Clear();
        }
    }
    private void ButtonRemoved(Button button)
    {
        if (!m_buttons.Contains(button))
        {
            //Debug.LogWarning($"Trying to remove button that does not exist");
            return;
        }
        m_buttons.Remove(button);
        if (m_buttons.Count > 0)
        {
            m_ui.Navigation.UpdateButtons(m_buttons, m_buttons[0].gameObject);
        }
        else
        {
            m_ui.Navigation.Clear();
        }

    }
    public override void Toggle(bool show)
    {
        if (show)
        {
            m_actionView.View();
            m_ui.PushUI(this);
        }
        else
        {
            m_actionView.Hide();
            m_ui.PopUI(this);
        }
    }
    public override bool IsVisible()
    {
        return m_actionView.gameObject.activeInHierarchy;
    }
}
