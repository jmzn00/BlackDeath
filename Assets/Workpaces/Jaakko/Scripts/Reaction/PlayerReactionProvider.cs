using Mono.Cecil;

public class PlayerReactionProvider : IReactionProvider
{
    public void TryReact(ReactiveWindow window, InputPrompt prompt) 
    {
        if (!prompt.action.WasPressedThisFrame()) return;

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
