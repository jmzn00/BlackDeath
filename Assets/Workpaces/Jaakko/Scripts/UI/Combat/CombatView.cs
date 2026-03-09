using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.ProBuilder.MeshOperations;
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

    private List<Button> m_currentButtons = new List<Button>();
    private CombatManager m_combatManager;
    private CombatActor m_currentActor;

    private CombatActor m_currentTarget = null;
    
    private UIManager m_uiManager;

    private Dictionary<CombatActor, CombatPortrait> m_actorPortraits = new();

    private Dictionary<string, Button> m_actionTypeButtons = new();
    private Dictionary<CombatAction, Button> m_actionButtons = new();    

    public void Initialize(CombatManager combatManager, UIManager uiManager)
    {
        m_combatManager = combatManager;
        m_uiManager = uiManager;

        m_combatManager.OnWindowOpened += OnWindowOpened;
        m_combatManager.OnWindowClosed += OnWindowClosed;

        m_combatManager.ReactiveWindow.OnParryWindowOpened += OnParryWindowOpened;
        m_combatManager.ReactiveWindow.OnDodgeWindowOpened += OnDodgeWindowOpened;

        m_dodgeImage.gameObject.SetActive(false);
        m_parryImage.gameObject.SetActive(false);
        m_confirmImage.gameObject.SetActive(false);
    }
    private void OnParryWindowOpened(bool value) 
    {
        if (m_currentActor.IsPlayer)
        {
            return;
        }
        if (value) 
        {
            m_parryImage.gameObject.SetActive(true);
        }
        else 
        {
            m_parryImage.gameObject.SetActive(false);
        }
    }
    private void OnDodgeWindowOpened(bool value) 
    {
        if (m_currentActor.IsPlayer)
        {
            return;
        }
        if (value) 
        {
            m_dodgeImage.gameObject.SetActive(true);
        }
        else 
        {
            m_dodgeImage.gameObject.SetActive(false);
        }
    }
    public void OnWindowOpened(ActionContext ctx)
    {
        SetAllButtonsInteractable(false);

        if (ctx.Source.IsPlayer)
        {
            m_confirmImage.gameObject.SetActive(true);
            m_confirmImage.sprite = ctx.Prompt.icon;
            // show confirm indication
        }
        
    }
    public void OnWindowClosed(ActionContext ctx)
    {
        SetAllButtonsInteractable(true);

        m_dodgeImage.gameObject.SetActive(false);
        m_parryImage.gameObject.SetActive(false);
        m_confirmImage.gameObject.SetActive(false);
    }
    public void OnContextChanged(CombatContext ctx)
    {
        
        var aliveActors = ctx.Actors.Where(a => !a.IsDead).ToList();

        // Remove dead portraits
        foreach (var kvp in m_actorPortraits.ToList())
        {
            if (!aliveActors.Contains(kvp.Key))
            {
                kvp.Value.Dispose();
                m_actorPortraits.Remove(kvp.Key);
            }
        }

        var currentSelected = EventSystem.current.currentSelectedGameObject;

        // Update existing portraits or create new
        foreach (var actor in aliveActors)
        {
            if (!m_actorPortraits.ContainsKey(actor))
            {
                CombatPortrait portrait = CreateTargetButton(actor);
                m_actorPortraits.Add(actor, portrait);
            }
            else
            {
                m_actorPortraits[actor].UpdateData(actor);
            }
        }

        // Set current actor
        m_currentActor = ctx.CurrentActor;

        // If player, show action types
        if (m_currentActor.IsPlayer)
        {
            PopulateActionTypes(m_currentActor);
        }
        else
        {
            // Optionally clear action types if it's an AI turn
            foreach (var btn in m_actionTypeButtons.Values)
                Destroy(btn.gameObject);
            m_actionTypeButtons.Clear();
            foreach (var btn in m_actionButtons.Values)
                Destroy(btn.gameObject);
            m_actionButtons.Clear();
        }

        bool isPlayerTurn = m_currentActor.IsPlayer;
        SetAllButtonsInteractable(isPlayerTurn);

        // Update navigation with all available buttons
        m_uiManager.Navigation.UpdateButtons(GetAllButtons(), currentSelected);
    }
    private void SetAllButtonsInteractable(bool interactable)
    {
        foreach (var btn in m_actorPortraits.Values.Select(p => p.GetComponent<Button>()))
            if (btn != null) btn.interactable = interactable;

        foreach (var btn in m_actionTypeButtons.Values)
            btn.interactable = interactable;

        foreach (var btn in m_actionButtons.Values)
            btn.interactable = interactable;
    }
    private CombatPortrait CreateTargetButton(CombatActor actor)
    {
        CombatPortrait p = Instantiate(m_combatPortraitPrefab, TargetsContainer);

        p.Initialize(actor, (clickedActor) =>
        {
            m_currentTarget = clickedActor;
        });

        return p;
    }
    public List<Selectable> GetAllButtons()
    {
        var allButtons = new List<Selectable>();

        // Targets
        allButtons.AddRange(m_actorPortraits.Values
            .Select(p => p.GetComponent<Selectable>()));

        // Action Types
        allButtons.AddRange(m_actionTypeButtons.Values
            .Select(b => b.GetComponent<Selectable>()));

        // Action Buttons
        allButtons.AddRange(m_actionButtons.Values
            .Select(b => b.GetComponent<Selectable>()));

        return allButtons.Where(b => b != null).ToList();
    }
    private void PopulateActionTypes(CombatActor actor)
    {
        // Clear previous
        foreach (var btn in m_actionTypeButtons.Values)
            Destroy(btn.gameObject);
        m_actionTypeButtons.Clear();

        var actions = actor.Actions;
        var attackActions = actions.OfType<AttackAction>().Cast<CombatAction>().ToList();
        var skillActions = actions.OfType<SkillAction>().Cast<CombatAction>().ToList();

        if (attackActions.Count > 0)
            CreateActionTypeButton("Attack", attackActions);

        if (skillActions.Count > 0)
            CreateActionTypeButton("Skill", skillActions);
    }
    private void CreateActionTypeButton(string label, List<CombatAction> actions)
    {
        Button button = Instantiate(typeButtonPrefab, actionTypeButtonContainer);
        button.GetComponentInChildren<TMP_Text>().text = label;

        button.onClick.AddListener(() => ShowActionButtons(actions));

        m_actionTypeButtons[label] = button;
    }
    private void ShowActionButtons(List<CombatAction> actions)
    {
        // Clear previous
        foreach (var btn in m_actionButtons.Values)
            Destroy(btn.gameObject);
        m_actionButtons.Clear();

        foreach (var action in actions)
        {
            Button button = Instantiate(actionButtonPrefab, actionButtonContainer);
            button.GetComponentInChildren<TMP_Text>().text = action.actionName;

            button.onClick.AddListener(() => OnActionSelected(action));
            m_actionButtons[action] = button;
        }

        // Update navigation so player can select with D-pad / keys
        m_uiManager.Navigation.UpdateButtons(m_actionButtons.Values, null);
    }
    private void OnActionSelected(CombatAction action)
    {
        if (m_currentTarget == null)
        {
            Debug.LogWarning("Select a target first");
            return;
        }

        ActionContext ctx = new ActionContext()
        {
            Action = action,
            Target = m_currentTarget
        };

        m_currentActor.ActionProvider.SetAction(ctx);
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

}