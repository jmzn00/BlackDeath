using System.Collections.Generic;

public class ActionContext
{
    public ActionContext()
    {

    }
    public CombatActor Source;
    public List<CombatActor> Targets = new();
    public CombatActor PrimaryTarget => Targets.Count > 0 ? Targets[0] : null;

    public CombatAction Action;

    public string PromptKey;
    public InputPrompt Prompt;
}