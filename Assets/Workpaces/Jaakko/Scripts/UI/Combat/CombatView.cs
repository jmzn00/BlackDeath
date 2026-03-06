using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class CombatView : MonoBehaviour, IUIComponentView
{
    [Header("Combat Order")]
    [SerializeField] private TMP_Text m_currentActorText;
    [SerializeField] private TMP_Text m_nextActor;
    [Header("UI Containers")]
    [SerializeField] private Transform actionTypeButtonContainer;
    [SerializeField] private Transform actionButtonContainer;
    [SerializeField] private Transform TargetsContainer;
    [Header("Prefabs")]
    [SerializeField] private Button typeButtonPrefab;
    [SerializeField] private Button actionButtonPrefab;
    [SerializeField] private Button targetButtonPrefab;

    private List<Button> m_currentButtons = new List<Button>();    
    private CombatManager m_combatManager;
    private CombatActor m_currentActor;

    private CombatActor m_currentTarget = null;
    public void OnContextChanged(CombatContext ctx) 
    {
        int count = ctx.Actors.Count;
        int currentIndex = ctx.TurnIndex % count;
        int nextIndex = (currentIndex + 1) % count;

        CombatActor current = ctx.Actors[currentIndex];
        if (!current.IsPlayer) 
        {
            m_currentTarget = null;
            ClearButtons();
        }
        

        m_currentActorText.text = "Current: " + current.name;

        m_nextActor.text = "Next: " + ctx.Actors[nextIndex].name;

        List<CombatActor> enemies = ctx.Actors.FindAll(a => !a.IsPlayer);
        foreach (var e in enemies)
            CreateTargetButton(e);
    }
    public void OnActorChanged(Actor actor) 
    {        
        ClearButtons();

        CombatActor combatActor = actor.Get<CombatActor>();
        m_currentActor = combatActor;
        var Actions = combatActor.Actions;
        var attackActions = Actions.OfType<AttackAction>().ToList<CombatAction>();
        var skillActions = Actions.OfType<SkillAction>().ToList<CombatAction>();

        if (attackActions.Count > 0) 
        {
            CreateButtonType("Attacks", attackActions);
        }
        if (skillActions.Count > 0) 
        {
            CreateButtonType("Skills", skillActions);
        }                
    }
    
    private void CreateTargetButton(CombatActor actor) 
    {
        Button button = Instantiate(targetButtonPrefab, TargetsContainer);
        TMP_Text text = button.GetComponentInChildren<TMP_Text>();
        text.text = actor.name;
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
