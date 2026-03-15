using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class CombatView : MonoBehaviour, IUIComponentView
{
    [Header("UI Containers")]
    [SerializeField] private Transform actionTypeButtonContainer;
    [SerializeField] private Transform actionButtonContainer;
    [SerializeField] private Transform TargetsContainer;
    [Header("Prefabs")]
    [SerializeField] private CombatPortrait m_combatPortraitPrefab;
    [SerializeField] private Button typeButtonPrefab;
    [SerializeField] private Button actionButtonPrefab;

    [Header("Parry / Dodge")]
    [SerializeField] private Image m_dodgeImage;
    [SerializeField] private Image m_parryImage;
    [SerializeField] private Image m_confirmImage;

    private CombatActor m_currentActor;
    private CombatActor m_currentTarget;
    private UIManager m_ui;

    private bool m_actionSet;

    private Dictionary<CombatActor, CombatPortrait> m_portraits = new();
    
    #region IUIComponentView
    public void Initialize(UIManager uiManager)
    {
        m_ui = uiManager;

        m_portraits = new Dictionary<CombatActor, CombatPortrait>();

        m_dodgeImage.gameObject.SetActive(false);
        m_parryImage.gameObject.SetActive(false);
        m_confirmImage.gameObject.SetActive(false);
    }
    public void OnActorChanged(Actor actor)
    {

    }
    public void Init()
    {

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
    #region ReactiveWindow
    public void OnParryWindowOpened(bool value)
    {
        if (m_currentActor.IsPlayer)
        {
            return;
        }
        m_parryImage.gameObject.SetActive(value);
    }
    public void OnDodgeWindowOpened(bool value)
    {
        if (m_currentActor.IsPlayer)
        {
            return;
        }
        m_dodgeImage.gameObject.SetActive(value);
    }
    public void OnConfirmWindowOpened(bool value)
    {
        if (!m_currentActor.IsPlayer)
        {
            return;
        }
        m_confirmImage.gameObject.SetActive(value);
    }
    public void OnWindowOpened(InputPrompt prompt) 
    {
        if (prompt != null)
            m_confirmImage.sprite = prompt.icon;
    }
    #endregion    
    public void ActorsChanged(List<CombatActor> actors) 
    {
        foreach (var a in actors) 
        {
            if (m_portraits.TryGetValue(a, out var p))
            {
                if (a.IsDead)
                {
                    m_portraits.Remove(a);
                    p.OnClick -= TargetSelected;

                    if (p.TryGetComponent<Selectable>(out var s))
                        m_ui.Navigation.RemoveSelectable(s);

                    p.Dispose();
                }
                else
                {
                    p.UpdateData(a);
                }
            }
            else
            {
                if (a.IsDead)
                    return;

                CombatPortrait portrait = Instantiate(m_combatPortraitPrefab
                    , TargetsContainer);
                portrait.Initialize(a);
                portrait.OnClick += TargetSelected;
                m_portraits.Add(a, portrait);
            }
        }
        UpdateNavigation();
    }
    public void TurnStarted(CombatActor actor) 
    {        
        m_currentActor = actor;
    }
    private void TargetSelected(CombatActor target) 
    {        
        m_currentTarget = target;
        CombatEvents.TargetSelected(m_currentActor, m_currentTarget);
        ShowActionTypes(m_currentActor);
    }
    private void ShowActionTypes(CombatActor actor)
    {
        ClearActionTypeButtons();

        var actions = actor.Actions;

        var attackActions = actions.OfType<AttackAction>().Cast<CombatAction>().ToList();
        var skillActions = actions.OfType<SkillAction>().Cast<CombatAction>().ToList();
        var skipActions = actions.OfType<SkipTurnAction>().Cast<CombatAction>().ToList();

        if (attackActions.Count > 0)
        {
            CreateActionTypeButton("Attack", attackActions);
        }
        if (skillActions.Count > 0)
        {
            CreateActionTypeButton("Skill", skillActions);
        }
        if (skipActions.Count > 0) 
        {
            CreateActionTypeButton("Skip", skipActions);
        }
    }
    private void CreateActionTypeButton(string label, List<CombatAction> actions)
    {
        Button b = Instantiate(typeButtonPrefab, actionTypeButtonContainer);
        b.GetComponentInChildren<TMP_Text>().text = label;

        b.onClick.AddListener(() =>
        {
            ShowActions(actions);
        });
        UpdateNavigation(b.gameObject);
    }
    private void ShowActions(List<CombatAction> actions)
    {
        ClearActionButtons();

        GameObject go = null;
        foreach (var a in actions)
        {
            Button b = Instantiate(actionButtonPrefab, actionButtonContainer);
            b.GetComponentInChildren<TMP_Text>().text = a.actionName;

            b.onClick.AddListener(() =>
            {
                OnActionSelected(a);
            });
            go = b.gameObject;
        }
        UpdateNavigation(go);
    }
    private void OnActionSelected(CombatAction action)
    {        
        m_currentActor.SubmitAction(m_currentActor,
            m_currentTarget, action);
    }
    private void ClearActionTypeButtons() 
    {
        foreach (Transform t in actionTypeButtonContainer) 
        {
            Button b = t.GetComponent<Button>();
            if (b)
                b.onClick.RemoveAllListeners();

            Destroy(t.gameObject);
        }        
        
        UpdateNavigation();
    }
    private void ClearActionButtons() 
    {
        foreach (Transform t in actionButtonContainer)
        {
            Button b = t.GetComponent<Button>();
            if (b)
                b.onClick.RemoveAllListeners();
            Destroy(t.gameObject);
        }
        UpdateNavigation();
    }
    public List<Selectable> GetSelectables() 
    {
        List<Selectable> result = new();
        foreach (var p in m_portraits.Values) 
        {
            var s = p.GetComponent<Selectable>();
            if (s != null)
                result.Add(s);
        }        
        foreach (Transform t in actionTypeButtonContainer) 
        {
            Selectable s = t.GetComponent<Selectable>();
            if (s)
                result.Add(s);
        }
        foreach (Transform t in actionButtonContainer) 
        {
            Selectable s = t.GetComponent<Selectable>();
            if (s)
                result.Add(s);
        }
        return result;
    }
    private void UpdateNavigation(GameObject current = null) 
    {
        Canvas.ForceUpdateCanvases();

        var buttons = GetSelectables();

        if (current == null) 
        {
            current = EventSystem.current.currentSelectedGameObject;
        }
        m_ui.Navigation.UpdateButtons(buttons, current);
    }
}