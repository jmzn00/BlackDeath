public interface IDamageSource
{
    string SourceName { get; }
    CombatActor SourceActor { get; }    
}