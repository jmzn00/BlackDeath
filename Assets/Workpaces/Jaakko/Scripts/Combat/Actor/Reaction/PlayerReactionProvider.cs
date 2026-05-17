using System;
using UnityEngine;

public class PlayerReactionProvider : IReactionProvider
{
    private CombatActor m_actor;
    public event Action<ReactionCommand> OnCommandReady;
    public PlayerReactionProvider(CombatActor actor) 
    {
        m_actor = actor;
    }
    public void OpenReaction() 
    {
    
    }
    public void TryReact(ReactionSystem reactionSystem, InputPrompt prompt)
    {
        if (prompt.inputType == PromptInputType.Parry || prompt.inputType == PromptInputType.Dodge)
            return;

        if (!prompt.action.WasPressedThisFrame())
            return;

        if (prompt.inputType == PromptInputType.Confirm)
            OnCommandReady?.Invoke(new ReactionCommand(m_actor, prompt));
    }
}
