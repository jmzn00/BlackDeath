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

public class ActionView : MonoBehaviour, IUIComponentView
{
    [Header("Prefabs")]
    [SerializeField] private CombatActionButton m_actionButtonPrefab;
    [SerializeField] private Button m_typeButtonPrefab;

    [Header("Anchors")]
    [SerializeField] private Transform m_typeAnchor;
    [SerializeField] private Transform m_actionAnchor;

    private Dictionary<Type, Button> m_actionTypeMap = new();

    public event Action<Button> OnButtonCreated;
    public event Action<Button> OnButtonRemoved;

    public event Action<Type> OnActionTypeSelected;
    public event Action<CombatAction> OnActionSelected;
    #region IUIComponentView
    public void View()
    {
        gameObject.SetActive(true);
    }
    public void Hide()
    {
        ClearActions();
        HideActionTypes();

        gameObject.SetActive(false);        
    }
    #endregion
    private void SetButtonActive(Button button, bool active) 
    {
        button.gameObject.SetActive(active);
        if (active)
            OnButtonCreated?.Invoke(button);
        else
            OnButtonRemoved?.Invoke(button);
    }
    public void Init() 
    {
        CreateActionTypeButton<SkillAction>("Skills");
        CreateActionTypeButton<AttackAction>("Attack");
        CreateActionTypeButton<SkipTurnAction>("Skip");
    }
    public void SetPosition(Vector3 position) 
    {
        transform.position = position;
    }
    public void ShowActionTypes(List<CombatAction> actions) 
    {
        HideActionTypes();

        var availableTypes = new HashSet<Type>();

        foreach (var action in actions) 
        {
            if (action is SkillAction) availableTypes.Add(typeof(SkillAction));
            else if (action is AttackAction) availableTypes.Add(typeof(AttackAction));
            else if (action is SkipTurnAction) availableTypes.Add(typeof(SkipTurnAction));
        }

        foreach (var type in availableTypes) 
        {
            if (m_actionTypeMap.TryGetValue(type, out var button)) 
            {
                SetButtonActive(button, true);
            }
        }
    }
    private void CreateActionTypeButton<T>(string label) where T : CombatAction
    {
        Button button = Instantiate(m_typeButtonPrefab, m_typeAnchor);
        TMP_Text text = button.GetComponentInChildren<TMP_Text>();
        text.text = label;

        button.onClick.AddListener(() =>
        {
            OnActionTypeSelected?.Invoke(typeof(T));
        });
        m_actionTypeMap[typeof(T)] = button;
        SetButtonActive(button, false);
        button.gameObject.name = "Button " + label;
    }
    public void ShowActionsOfType(Type type, List<CombatAction> actions) 
    {
        HideActionTypes();
        ClearActions();
        foreach (var action in actions) 
        {
            if (action.GetType() != type) continue;

            var combatActionButton
                = Instantiate(m_actionButtonPrefab, m_actionAnchor);

            combatActionButton.UpdateAction(action);

            Button button = combatActionButton.GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                OnActionSelected?.Invoke(action);
            });
            SetButtonActive(button, true);
        }  
    }
    private void HideActionTypes()
    {
        foreach (var kvp in m_actionTypeMap)
        {
            SetButtonActive(kvp.Value, false);
        }
    }
    public void ClearActions()
    {
        foreach (Transform child in m_actionAnchor)
        {
            Button b = child.gameObject.GetComponent<Button>();
            if (b != null) 
            {
                b.onClick.RemoveAllListeners();
                SetButtonActive(b, false);
            }            
            Destroy(child.gameObject);
        }
    }
}
