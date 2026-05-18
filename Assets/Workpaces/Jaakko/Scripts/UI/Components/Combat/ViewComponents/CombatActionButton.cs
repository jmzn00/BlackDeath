using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class CombatActionButton : MonoBehaviour
{
    [SerializeField] private TMP_Text m_actionNameText;
    [SerializeField] private TMP_Text m_actionDesciptionText;
    [SerializeField] private TMP_Text m_apCostText;

    public void UpdateAction(CombatAction action, CombatActor actor) 
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
            }            
        }
        m_actionDesciptionText.text = description;

        if (action.apCost > 0)
        {
            m_apCostText.text = action.apCost.ToString();

            bool hasAp = action.apCost <= actor.ActionPoints;

            m_apCostText.color = hasAp ? Color.green : Color.red;
            Debug.Log($"HasAp {hasAp}");
        }
        name = "Button " + action.actionName;        
    }
}
