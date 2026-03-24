public class ReactionCommand : ICombatCommand 
{
    public CombatActor Source { get; }
    public InputPrompt Prompt { get; }
    public ReactionCommand(CombatActor source, InputPrompt prompt) 
    {
        Source = source;
        Prompt = prompt;
    }
}