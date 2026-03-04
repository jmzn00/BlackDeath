using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Base class for enemy AI. Create subclasses as ScriptableObjects.
/// </summary>
public abstract class CombatBehavior : ScriptableObject
{
    public abstract CombatAction ChooseAction(Combatant self, BattleManager manager);

    protected List<Combatant> GetOpponents(Combatant self, BattleManager manager)
    {
        var list = new List<Combatant>();
        foreach (var b in manager.GetBattlers())
        {
            if (b == null || b == self) continue;
            if (b.gameObject.activeSelf && b.health > 0) list.Add(b);
        }
        return list;
    }
}
