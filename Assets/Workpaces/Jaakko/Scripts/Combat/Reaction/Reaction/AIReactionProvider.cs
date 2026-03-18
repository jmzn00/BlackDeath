using System.Collections.Generic;
using UnityEngine;

public class AIReactionProvider : IReactionProvider
{
    private int dodgePercentage;
    private int parryPercentage;
    private int confirmPercentage;



    private CombatActor m_actor;
    public AIReactionProvider(AiReactionSettings settings, CombatActor actor)
    {
        m_actor = actor;

        if (settings == null)
        {
            Debug.LogWarning("[AIReactionProvider] settings is null — using zero chances.");
            dodgePercentage = parryPercentage = confirmPercentage = 0;
        }
        else
        {
            dodgePercentage = Mathf.Clamp(settings.dodgePercentage, 0, 100);
            parryPercentage = Mathf.Clamp(settings.parryPercentage, 0, 100);
            confirmPercentage = Mathf.Clamp(settings.confirmPercentage, 0, 100);
        }
    }
    public void TryReact(ReactionSystem reactionSystem, InputPrompt prompt)
    {
        if (reactionSystem == null || prompt == null) return;
        /*
        float roll = Random.value * 100f;

        switch (prompt.inputType)
        {
            case PromptInputType.Parry:
                if (roll < parryPercentage) 
                {
                    reactionSystem.ReceiveReaction(m_actor, prompt);
                    m_actor.OnDodgePerformed();
                }                
                break;
            case PromptInputType.Dodge:
                if (roll < dodgePercentage) 
                {
                    reactionSystem.ReceiveReaction(m_actor, prompt);
                    m_actor.OnParryPerformed();
                }                
                break;
            case PromptInputType.Confirm:
                if (roll < confirmPercentage)
                    reactionSystem.ReceiveReaction(m_actor, prompt);
                break;
        }   
        */
    }
}
