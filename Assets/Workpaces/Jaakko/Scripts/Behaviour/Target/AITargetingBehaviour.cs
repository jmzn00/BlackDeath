using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public abstract class AITargetingBehaviour : ScriptableObject
{
    public abstract float Evaluate(CombatActor actor,
        List<CombatActor> participants,
        out CombatActor selected);
}
