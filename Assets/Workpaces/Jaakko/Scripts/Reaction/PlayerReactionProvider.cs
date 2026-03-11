using UnityEngine;

public class PlayerReactionProvider : IReactionProvider
{
    private CombatActor m_actor;
    public PlayerReactionProvider(CombatActor actor) 
    {
        m_actor = actor;
    }
    public void TryReact(ReactiveWindow window, InputPrompt prompt) 
    {
        /*
        if (m_actor.DefensiveAnimationPlaying) 
        {
            Debug.Log("PR: DefensiveAnimPlaying");
            return;
        }
        */
        if (!prompt.action.WasPressedThisFrame()) 
        {
            return;
        }

        switch (prompt.inputType) 
        {
            case PromptInputType.Confirm:
                window.TryActivateConfirm();
                break;
            case PromptInputType.Dodge:
                window.TryActivateDodge();
                break;
            case PromptInputType.Parry:
                window.TryActivateParry();
                break;
        }
    }
}
