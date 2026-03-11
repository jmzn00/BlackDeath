using System.Collections.Generic;
using UnityEngine;

public class AIReactionProvider : IReactionProvider
{
    private int dodgePercentage;
    private int parryPercentage;
    private int confirmPercentage;

    private readonly HashSet<InputPrompt> m_rolledPrompts = new();

    private bool m_subscribed = false;

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

    public void TryReact(ReactiveWindow window, InputPrompt prompt)
    {
        if (window == null || prompt == null) return;

        if (!m_subscribed)
        {
            window.OnWindowClosed += ctx =>
            {
                m_rolledPrompts.Clear();
            };
            m_subscribed = true;
        }

        if (m_rolledPrompts.Contains(prompt)) return;

        float roll = Random.value * 100f;
        //Debug.Log($"[AIReactionProvider] Roll {roll:0.00} for prompt {prompt.name} ({prompt.inputType})");

        switch (prompt.inputType)
        {
            case PromptInputType.Parry:
                if (roll < parryPercentage) 
                {
                    window.TryActivateParry();
                    m_actor.OnDodgePerformed();
                }                
                break;
            case PromptInputType.Dodge:
                if (roll < dodgePercentage) 
                {
                    window.TryActivateDodge();
                    m_actor.OnParryPerformed();
                }
                
                break;
            case PromptInputType.Confirm:
                if (roll < confirmPercentage)
                    window.TryActivateConfirm();
                break;
        }
        m_rolledPrompts.Add(prompt);
    }
}
