public class PlayerReactionProvider : IReactionProvider
{
    private CombatActor m_actor;
    public PlayerReactionProvider(CombatActor actor) 
    {
        m_actor = actor;
    }
    public void TryReact(ReactionSystem reactionSystem, InputPrompt prompt) 
    {
        if (!prompt.action.WasPressedThisFrame()) 
        {
            return;
        }
        reactionSystem.ReceiveReaction(m_actor, prompt);        
    }
}
