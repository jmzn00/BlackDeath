using NUnit.Framework;
using System.Collections.Generic;

public class AttackCommand : ICombatCommand 
{
    public CombatActor Source { get; }
    public CombatActor Target { get; }
    public CombatAction Action { get; }
    public List<CombatActor> Targets { get; }
    public AttackCommand(CombatActor source, CombatActor target, CombatAction action
        , List<CombatActor> targets) 
    {
        Source = source;
        Action = action;

        Target = target;
        Targets = targets;
    }
}