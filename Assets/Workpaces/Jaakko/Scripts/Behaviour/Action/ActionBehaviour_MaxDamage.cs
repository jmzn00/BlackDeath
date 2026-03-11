using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CombatActor/ActionBehaviours/MaxDamage")]
public class ActionBehaviour_MaxDamage : AIActionBehaviour
{
    public override float Evaluate(CombatActor actor,
        List<CombatActor> participants,
        out CombatAction selectedAction)
    {
        selectedAction = null;

        CombatAction bestAction = null;
        float highestDamage = 0f;
        for (int i = 0; i < actor.Actions.Count; i++) 
        {
            float damage = actor.Actions[i].baseDamage;
            if (damage > highestDamage) 
            {
                bestAction = actor.Actions[i];
                highestDamage = damage;
            }
        }
        selectedAction = bestAction;
        return 1f;       
    }
}
