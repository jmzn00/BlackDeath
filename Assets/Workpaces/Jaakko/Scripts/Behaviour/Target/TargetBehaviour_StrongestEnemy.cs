using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CombatActor/TargetBehaviours/StrongestEnemy")]
public class TargetBehaviour_StrongestEnemy : AITargetingBehaviour
{
    public override float Evaluate(CombatActor actor,
    List<CombatActor> participants, CombatAction action,
    out CombatActor selected)
    {
        selected = null;

        var enemies = action.GetValidTargets(actor, participants);
        if (enemies.Count == 0) 
        {
            Debug.LogWarning($"No Targets {name}");
            return 0f;
        }

        float highestHp = 0f;
        CombatActor bestTarget = null;
        foreach (var enemy in enemies)
        {
            float hp = enemy.Health.CurrentHealth;
            if (hp > highestHp)
            {
                highestHp = hp;
                bestTarget = enemy;
            }
        }
        if (bestTarget != null)
            selected = bestTarget;

        return 1f;
    }
}
