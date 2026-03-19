using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class CombatActionButton : MonoBehaviour
{
    [SerializeField] private TMP_Text m_actionNameText;
    [SerializeField] private TMP_Text m_actionDesciptionText;

    public event Action<CombatAction> OnActionSelected;

    public void UpdateAction(CombatAction action) 
    {
        m_actionNameText.text = action.actionName;

        string description = $"Applies {action.baseDamage} damage with " +
            $"multiplier {action.confirmDamageMultipler} on confirm.";

        if (action.AppliedEffects.Count > 0) 
        {
            description += " Applies ";
            foreach (var e in action.AppliedEffects) 
            {
                description += $"{e.displayName} for {e.duration} turns.";

                if (e.IsStackable) 
                {
                    description += $"{e.displayName} can be stacked";
                }
                else 
                {
                    description += $"{e.displayName} can not be stacked";
                }
            }
        }
        m_actionDesciptionText.text = description;
        name = "Button " + action.actionName;
    }
}
