public class AttackCommand : ICombatCommand 
{
    public CombatActor Source { get; }
    public CombatActor Target { get; }
    public CombatAction Action { get; }
    public AttackCommand(CombatActor source, CombatActor target, CombatAction action) 
    {
        Source = source;
        Target = target;
        Action = action;
    }
}