using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class CombatView : MonoBehaviour, IUIComponentView
{
    private enum ActionViewState
    {
        TypeSelection,
        ActionSelection
    }
    private ActionViewState m_currentViewState = ActionViewState.TypeSelection;
    private Type m_currentActionType;

    [Header("UI Containers")]
    [SerializeField] private Transform actionTypeButtonContainer;
    [SerializeField] private Transform actionButtonContainer;
    [SerializeField] private Transform TargetsContainer;
    [Header("Prefabs")]
    [SerializeField] private CombatPortrait m_combatPortraitPrefab;
    [SerializeField] private Button typeButtonPrefab;
    [SerializeField] private Button actionButtonPrefab;

    [Header("Parry / Dodge")]
    [SerializeField] private Image m_confirmImage;

    private CombatActor m_currentActor;
    private List<CombatActor> m_participants;    
    private UIManager m_ui;

    private CombatAction m_currentAction;
    private CombatActor m_currentTarget;

    private List<Selectable> m_currentNavigateableButtons = new();


    #region IUIComponentView
    public void Initialize(UIManager uiManager)
    {
        m_ui = uiManager;
    }
    public void OnActorChanged(Actor actor)
    {

    }
    private Dictionary<Type, Button> m_actionTypeMap = new();
    public void Init()
    {
        CreateActionTypeButton<SkillAction>("Skills");
        CreateActionTypeButton<AttackAction>("Attacks");
        CreateActionTypeButton<SkipTurnAction>("Skip");
    }
    public void View()
    {
        gameObject.SetActive(true);
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }
    #endregion
    public void ActorsChanged(List<CombatActor> actors) 
    {
        m_participants = new List<CombatActor>(actors);
    }
    public void BackPressed() 
    {
        ClearActionButtons();
        ShowActionTypeButtons(true);
    }
    public void TurnStarted(CombatActor actor) 
    {        
        m_currentActor = actor;
        SelectAction();
    }
    public void TurnEnded(CombatActor actor) 
    {
        m_ui.Navigation.Clear();

        m_currentActor = null;
        m_currentTarget = null; 
        m_currentAction = null; 
    }
    private Dictionary<Type, List<CombatAction>> m_cachedActions = new();

    public void SubmitAction() 
    {
        if (m_currentAction == null || m_currentTarget == null) 
        {
            Debug.LogWarning("Cannot Submit: Target || Action is NULL");
            return;
        }
        ActionContext ctx = new ActionContext
        {
            Action = m_currentAction,
            Source = m_currentActor,
            Target = m_currentTarget
        };
        m_currentActor.ActionProvider.SetAction(ctx);
    }
    private void SelectAction() 
    {
        m_currentActor.ChangeState(CombatActorState.ActionSelecting);

        List<CombatAction> actions = m_currentActor.Actions;
        if (actions.Count == 0) 
        {
            Debug.LogWarning($"{m_currentActor.name} has no actions");
            return;
        }
        
        foreach (var kvp in m_actionTypeMap) 
        {
            kvp.Value.gameObject.SetActive(false);
        }
        m_cachedActions.Clear();
        foreach (var action in actions) 
        {
            Type type = null;

            if (action is SkillAction) type = typeof(SkillAction);
            else if (action is AttackAction) type = typeof(AttackAction);
            else if (action is SkipTurnAction) type = typeof(SkipTurnAction);

            if (type == null) continue;


            if (!m_cachedActions.ContainsKey(type)) 
            {
                m_cachedActions[type] = new List<CombatAction>();
            }
            m_cachedActions[type].Add(action);
        }
        ShowActionTypeButtons(true);
    }
    private void ActionSelected(CombatAction action)
    {
        m_currentAction = action;
        SelectTarget();
    }
    private void SelectTarget() 
    {
        m_currentActor.ChangeState(CombatActorState.Targeting);
        m_currentTarget = m_participants[0];
        m_currentActor.ChangeTarget(m_currentTarget);
    }
    private bool m_horizontalUsedLastFrame = false;
    public void TargetScroll(float value)
    {
        if (m_currentActor == null) { return; }
        if (m_currentAction == null) { return; }
        if (m_currentTarget == null) { return; }

        if (!m_horizontalUsedLastFrame)
        {
            int currentIndex = m_participants.IndexOf(m_currentTarget);
            if (currentIndex == -1) return;

            if (value >= 0.5f)
            {
                currentIndex = (currentIndex + 1) % m_participants.Count;
                m_horizontalUsedLastFrame = true;
            }
            else if (value <= -0.5f)
            {
                currentIndex = (currentIndex - 1 + m_participants.Count) % m_participants.Count;
                m_horizontalUsedLastFrame = true;
            }

            m_currentTarget = m_participants[currentIndex];
            m_currentActor.ChangeTarget(m_currentTarget);
        }

        if (Mathf.Abs(value) < 0.5f)
            m_horizontalUsedLastFrame = false;
    }
    private void ShowActionTypeButtons(bool value) 
    {
        m_currentViewState = ActionViewState.TypeSelection;
        m_currentNavigateableButtons.Clear();

        foreach (var kvp in m_cachedActions)
        {
            if (m_actionTypeMap.TryGetValue(kvp.Key, out var button))
            {
                button.gameObject.SetActive(value);
                if (value)
                    m_currentNavigateableButtons.Add(button);
            }
        }

        if (m_currentNavigateableButtons.Count > 0) 
        {
            m_ui.Navigation.UpdateButtons(m_currentNavigateableButtons
                , m_currentNavigateableButtons[0].gameObject);
        }
    }
    private void ShowActionsOfType(Type type)
    {
        m_currentViewState = ActionViewState.ActionSelection;
        ClearActionButtons();
        m_currentNavigateableButtons.Clear();

        if (!m_cachedActions.TryGetValue(type, out var actions))
            return;

        foreach (var action in actions)
        {
            Button button = Instantiate(actionButtonPrefab, actionButtonContainer);
            TMP_Text text = button.GetComponentInChildren<TMP_Text>();
            text.text = action.actionName;

            button.onClick.AddListener(() =>
            {
                ActionSelected(action);
            });
            m_currentNavigateableButtons.Add(button);
        }

        if (m_currentNavigateableButtons.Count > 0) 
        {
            m_ui.Navigation.UpdateButtons(m_currentNavigateableButtons
                , m_currentNavigateableButtons[0].gameObject);
        }
    }
    private void CreateActionTypeButton<T>(string label) where T : CombatAction
    {
        Button button = Instantiate(typeButtonPrefab, actionTypeButtonContainer);
        TMP_Text text = button.GetComponentInChildren<TMP_Text>();
        text.text = label;

        button.onClick.AddListener(() =>
        {
            ShowActionsOfType(typeof(T));
        });
        button.gameObject.SetActive(false);
        m_actionTypeMap[typeof(T)] = button;
    }
    private void ClearActionButtons() 
    {
        foreach (Transform child in actionButtonContainer) 
        {
            Destroy(child.gameObject);
        }
    }
}