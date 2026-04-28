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
        if (!prompt.action.WasPressedThisFrame()) 
        {
            return;
        }

        bool success = false;
        if (prompt.inputType == PromptInputType.Dodge) 
        {
            if (m_actor.Animator.DodgeOpen) 
            {
                success = true;
            }
        }
        if (prompt.inputType == PromptInputType.Parry) 
        {
            if (m_actor.Animator.ParryOpen) 
            {
                success = true;
            }
        }
        if (success)
            OnCommandReady?.Invoke(new ReactionCommand(m_actor, prompt));
    }
}
