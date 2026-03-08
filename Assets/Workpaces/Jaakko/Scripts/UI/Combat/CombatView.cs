using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CombatView : MonoBehaviour, IUIComponentView
{
    [Header("UI Containers")]
    [SerializeField] private Transform actionTypeButtonContainer;
    [SerializeField] private Transform actionButtonContainer;
    [SerializeField] private Transform TargetsContainer;
    [Header("Prefabs")]
    [SerializeField] private Button typeButtonPrefab;
    [SerializeField] private Button actionButtonPrefab;
    [SerializeField] private Button targetButtonPrefab;

    [Header("Parry / Dodge")]
    [SerializeField] private GameObject parryIndicator;
    [SerializeField] private GameObject dodgeIndicator;

    private List<Button> m_currentButtons = new List<Button>();    
    private CombatManager m_combatManager;
    private CombatActor m_currentActor;

    private CombatActor m_currentTarget = null;

    private void Update()
    {
        if (m_combatManager == null) return;
        if (m_combatManager.State == CombatState.None) return;
        if (m_currentActor == null || m_currentActor.IsPlayer) return;

        ReactiveWindow window = m_combatManager.ReactiveWindow;

        parryIndicator.SetActive(window.CanParry);
        dodgeIndicator.SetActive(window.CanDodge);
    }
    public void OnContextChanged(CombatContext ctx) 
    {
        ClearButtons();
        m_currentActor = ctx.CurrentActor;
        if (!ctx.CurrentActor.IsPlayer || ctx.CurrentActor.IsDead) 
        {
            // hide action selection
            return;
        }        
        var Actions = ctx.CurrentActor.Actions;
        var AttackActions = Actions.OfType<AttackAction>().ToList<CombatAction>();
        var SkillActions = Actions.OfType<SkillAction>().ToList<CombatAction>();

        if (AttackActions.Count > 0) 
        {
            CreateButtonType("Attacks", AttackActions);
        }
        if (SkillActions.Count > 0) 
        {
            CreateButtonType("Skills", SkillActions);
        }

        List<CombatActor> aliveEnemies = ctx.Actors.Where(a => a != a.IsPlayer && a != a.IsDead).ToList();
        foreach(var e in aliveEnemies) 
        {
            CreateTargetButton(e);
        }        
    }
    public void OnActorChanged(Actor actor) 
    {     

    }
    private void CreateTargetButton(CombatActor actor) 
    {
        Button button = Instantiate(targetButtonPrefab, TargetsContainer);
        TMP_Text text = button.GetComponentInChildren<TMP_Text>();
        text.text = $"{actor.name} H:{actor.Health.GetHealth()}";
        m_currentButtons.Add(button);

        button.onClick.AddListener(() =>
        {
            m_currentTarget = actor;
        });
    }
    private void CreateButtonType(string label, List<CombatAction> actions) 
    {
        Button button = Instantiate(typeButtonPrefab, actionTypeButtonContainer);
        button.GetComponentInChildren<TMP_Text>().text = label;
        button.onClick.AddListener(() => ShowActionButtons(actions));
        m_currentButtons.Add(button);
    }
    private void ShowActionButtons(List<CombatAction> actions) 
    {
        foreach (Transform child in actionButtonContainer)
            Destroy(child.gameObject);

        foreach (var action in actions) 
        {
            Button button = Instantiate(actionButtonPrefab, actionButtonContainer);
            button.GetComponentInChildren<TMP_Text>().text = action.actionName;

            button.onClick.AddListener(() =>
            {
                if (m_currentTarget == null) 
                {
                    Debug.LogWarning("Current Target is NULL");
                    return;
                }
                ActionContext ctx = new ActionContext()
                {                    
                    Action = action,
                    Target = m_currentTarget
                    
                };
                m_currentActor.ActionProvider.SetAction(ctx);                             
            });
        }
    }    
    private void ClearButtons() 
    {
        foreach (var btn in m_currentButtons)
            Destroy(btn.gameObject);
        m_currentButtons.Clear();

        foreach (Transform child in actionButtonContainer)
            Destroy(child.gameObject);
    }
    public void Init(Actor actor) 
    {
        m_combatManager = actor.Game.Resolve<CombatManager>();
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
