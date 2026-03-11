using System.Collections.Generic;
using UnityEngine;

public abstract class AIActionBehaviour : ScriptableObject 
{
    public abstract float Evaluate
        (CombatActor actor,
        List<CombatActor> participants,
        out CombatAction selectedAction);
}
