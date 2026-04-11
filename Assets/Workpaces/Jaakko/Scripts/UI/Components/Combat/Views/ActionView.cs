using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public enum ActionViewState
{
    ActionType,
    ActionSelect,
    Selected
}

public class ActionView : UIViewBase
{
    [Header("Prefabs")]
    [SerializeField] private CombatActionButton m_actionButtonPrefab;
    [SerializeField] private Button m_typeButtonPrefab;

    [Header("Anchors")]
    [SerializeField] private Transform m_typeAnchor;
    [SerializeField] private Transform m_actionAnchor;

    [Header("Position")]
    [SerializeField] private Vector3 m_positionOffset;    

    public event Action<Type> OnActionTypeSelected;
    public event Action<CombatAction> OnActionSelected;

    private Dictionary<Type, Button> m_typeButtons;
    private Dictionary<CombatActionButton, Button> m_actionButtons;
    public override void Init() 
    {
        m_typeButtons = new();
        m_actionButtons = new();

        CreateTypeButton(typeof(SkillAction), "SkillAction");
        CreateTypeButton(typeof(AttackAction), "AttackAction");
        CreateTypeButton(typeof(SkipTurnAction), "SkipAction");

        for (int i = 0; i < 6; i++) 
        {
            CreateActionButton();
        }        
    }
    public void SetPosition(Vector3 position) 
    {
        transform.position = position + m_positionOffset;
    }
    private void CreateActionButton() 
    {
        CombatActionButton cab = Instantiate(m_actionButtonPrefab, m_actionAnchor);
        Button button = cab.GetComponent<Button>();

        TMP_Text text = cab.GetComponentInChildren<TMP_Text>();
        if (text != null)
            text.ForceMeshUpdate(); // build mesh once

        m_actionButtons[cab] = button;

        cab.gameObject.SetActive(false);
    }
    private void CreateTypeButton(Type type, string label) 
    {
        Button button = Instantiate(m_typeButtonPrefab, m_typeAnchor);
        TMP_Text text = button.GetComponentInChildren<TMP_Text>();
        if (text != null)
            text.SetText(label);

        button.onClick.AddListener(() =>
        {
            OnActionTypeSelected?.Invoke(type);
        });

        m_typeButtons[type] = button;
        ToggleButton(button, false);        
    }

    public void ShowTypeButtons(List<CombatAction> actions) 
    {
        HideTypeButtons();
        HideActionButtons();
        
        HashSet<Type> availableTypes = new();

        foreach (var action in actions) 
        {
            availableTypes.Add(action.GetType());
        }        
        foreach (var type in availableTypes) 
        {
            if (m_typeButtons.TryGetValue(type, out Button button)) 
            {
                ToggleButton(button, true);
            }
        }                
    }
    public void ShowActionsOfType(Type type, List<CombatAction> actions) 
    {
        HideTypeButtons();
        HideActionButtons();

        int index = 0;

        foreach (var action in actions) 
        {
            if (!type.IsAssignableFrom(action.GetType()))
                continue;

            if (index >= m_actionButtons.Count) 
            {
                // should spawn more
                break;
            }

            var pair = new List<KeyValuePair<CombatActionButton
                , Button>>(m_actionButtons)[index];

            CombatActionButton cab = pair.Key;
            Button button = pair.Value;

            cab.UpdateAction(action);

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                OnActionSelected?.Invoke(action);
            });

            cab.gameObject.SetActive(true);
            ToggleButton(button, true);

            index++;
        }
    }
    private void HideTypeButtons()
    {
        foreach (var btn in m_typeButtons.Values)
            ToggleButton(btn, false);
    }
    private void HideActionButtons()
    {
        foreach (var btn in m_actionButtons.Values)
            ToggleButton(btn, false);
    }    
}
