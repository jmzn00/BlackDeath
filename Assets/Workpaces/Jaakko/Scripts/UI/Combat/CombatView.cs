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

    [SerializeField] private CombatPortrait m_combatPortraitPrefab;

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
            // dispose any existing portraits safely and clear list
            foreach (var p in m_portraits.ToList())
            {
                if (p != null)
                    p.Dispose();
            }
            m_portraits.Clear();
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

        // dispose previous portraits safely and clear list
        foreach (var p in m_portraits.ToList())
        {
            if (p != null)
                p.Dispose();
        }
        m_portraits.Clear();

        // fixed filter: select non-player, non-dead actors
        List<CombatActor> aliveEnemies = ctx.Actors.Where(a => !a.IsPlayer && !a.IsDead).ToList();
        foreach (var e in aliveEnemies)
        {
            CreateTargetButton(e);
        }
    }
    public void OnActorChanged(Actor actor)
    {

    }
    private List<CombatPortrait> m_portraits = new List<CombatPortrait>();
    private void CreateTargetButton(CombatActor actor)
    {
        CombatPortrait p = Instantiate(m_combatPortraitPrefab, TargetsContainer);

        p.Initialize(actor, (clickedActor) =>
        {
            m_currentTarget = clickedActor;
        });
        m_portraits.Add(p);
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

        foreach (Transform child in actionButtonContainer)
            Destroy(child.gameObject);

        m_currentButtons.Clear();
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