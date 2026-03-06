using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CombatView : MonoBehaviour, IUIComponentView
{
    [SerializeField] private TMP_Text m_currentActorText;
    [SerializeField] private TMP_Text m_nextActor;

    [Header("UI Containers")]
    [SerializeField] private Transform actionTypeButtonContainer;
    [SerializeField] private Transform actionButtonContainer;

    [Header("Prefabs")]
    [SerializeField] private Button typeButtonPrefab;
    [SerializeField] private Button actionButtonPrefab;

    private List<Button> m_currentButtons = new List<Button>();

    private CombatActor m_currentActor;

    public void OnContextChanged(CombatContext ctx) 
    {
        int count = ctx.Actors.Count;
        int currentIndex = ctx.TurnIndex % count;
        int nextIndex = (currentIndex + 1) % count;

        CombatActor current = ctx.Actors[currentIndex];
        if (!current.IsPlayer)
            ClearButtons();

        m_currentActorText.text = "Current: " + current.name;

        m_nextActor.text = "Next: " + ctx.Actors[nextIndex].name;


    }
    public void OnActorChanged(Actor actor) 
    {
        CombatActor combatActor = actor.Get<CombatActor>();
        m_currentActor = combatActor;

        // this probably never happens
        if (!combatActor.IsPlayer) 
        {
            return;
        }

        ClearButtons();

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
                ActionContext ctx = new ActionContext()
                {
                    Action = action
                };
                if (!m_currentActor.SetActionContext(ctx)) 
                {
                    
                }
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
