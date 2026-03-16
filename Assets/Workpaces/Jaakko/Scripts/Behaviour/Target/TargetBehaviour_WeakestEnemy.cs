using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CombatActor/TargetBehaviours/WeakestEnemy")]
public class TargetBehaviour_WeakestEnemy : AITargetingBehaviour
{
    public override float Evaluate(CombatActor actor,
        List<CombatActor> participants, out CombatActor selected)
    {
        selected = null;

        var enemies = participants.FindAll(a => a.IsPlayer && !a.IsDead);
        if (enemies.Count == 0)
            return 0f;

        float lowestHp = 100f;
        CombatActor bestTarget = null;
        foreach (var enemy in enemies) 
        {
            float hp = enemy.Health.CurrentHealth;
            if (hp < lowestHp) 
            {
                lowestHp = hp;
                bestTarget = enemy;
            }
        }
        if (bestTarget != null)
            selected = bestTarget;

        return 1f;
    }
}
