using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;
using System.Linq;
public class CombatUI : UIComponentBase<CombatUIViewGroup>
{
    private InputManager m_input;
    private UIManager m_ui;

    private ActionView m_actionView;
    private TargetView m_targetView;
    private DamageView m_damageView;

    private ActionViewState m_actionViewState;
    private List<Button> m_buttons = new();

    private CombatActor m_currentActor;

    private CombatAction m_currentAction;
    private CombatActor m_currentTarget;

    private List<CombatActor> m_currentEnemies;
    private int m_currentTargetIndex = 0;
    public CombatUI(GameManager game, CombatUIViewGroup group) 
        : base(game, group)
    {
        m_actionView = group.ActionView;
        m_targetView = group.TargetView;
        m_damageView = group.DamageView;

        m_targetView.Hide();
        m_actionView.Hide();
        m_damageView.Hide();

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

        m_actionView.Init();
        m_targetView.Init();
        m_damageView.Init();

        m_input.OnSelectTarget += SelectTarget;
        m_input.OnUIInputAction += OnUIInputAction;
    }
    private void OnUIInputAction(UIInputAction action) 
    {
        switch (action) 
        {
            case UIInputAction.Submit:
                SubmitAction();
                break;
            case UIInputAction.Cancel:
                GoBack();
                break;
        }
    }
    public override void Dispose()
    {
        m_actionView.OnButtonCreated -= ButtonCreated;
        m_actionView.OnButtonRemoved -= ButtonRemoved;

        m_input.OnUIInputAction -= OnUIInputAction;
        m_input.OnSelectTarget -= SelectTarget;

        CombatEvents.OnTurnStarted -= TurnStart;
        CombatEvents.OnTurnEnded -= TurnEnd;
        CombatEvents.OnCombatActorsChanged -= ActorsChanged;
    }
    private void ActorsChanged(List<CombatActor> actors) 
    {
        m_currentEnemies = new List<CombatActor>();

        foreach (CombatActor actor in actors) 
        {
            if (actor.Team == Team.Enemy)
                m_currentEnemies.Add(actor);
        }
    }
    private void SelectTarget(float value) 
    {
        if (m_currentEnemies == null) 
        {
            Debug.LogWarning($"Cannot Select Target, enemies is NULL");
            return;
        }
        if (m_currentAction == null) 
        {
            Debug.LogWarning($"Cannot Select Target, Action is NULL");
            return;
        }

        List<CombatActor> validTargets 
            = new List<CombatActor>(m_currentEnemies)
            .Where(e => !e.IsDead).ToList();
        
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
        m_currentActor.ChangeTarget(m_currentTarget);
    }
    private void SubmitAction() 
    {
        if (m_currentActor == null
            || m_currentAction == null
            || m_currentTarget == null) return;

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
            
        switch (m_actionViewState) 
        {
            case ActionViewState.ActionType:

                break;
            case ActionViewState.ActionSelect:
                m_actionView.ClearActions();
                m_actionView.ShowActionTypes(m_currentActor.Actions);
                break;
            case ActionViewState.Selected:

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

        m_actionViewState = ActionViewState.ActionType;        

        // for camera
        m_currentActor.ChangeState(CombatActorState.ActionSelecting); 
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
        m_actionViewState = ActionViewState.ActionSelect;
    }
    private void ActionSelected(CombatAction action) 
    {
        m_actionView.Hide();
        m_targetView.View();
        SelectTarget(0f);

        m_currentAction = action;
        m_currentActor.ChangeState(CombatActorState.Targeting);
        m_actionViewState = ActionViewState.Selected;
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
            Debug.LogWarning($"Trying to remove button that does not exist");
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
        }
        else 
        {
            m_actionView.Hide();
        }
    }
    public override bool IsVisible()
    {
        return m_actionView.gameObject.activeInHierarchy;
    }
}
