using System;

public interface IReactionProvider
{
    event Action<ReactionCommand> OnCommandReady;
    // for ai so its doesn not roll multiple times
    void OpenReaction();
    void TryReact(ReactionSystem reactionSystem, InputPrompt prompt);    
}
