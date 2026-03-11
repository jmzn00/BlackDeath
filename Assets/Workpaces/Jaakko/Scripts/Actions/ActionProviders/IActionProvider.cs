using System.Collections.Generic;
public interface IActionProvider
{
    void RequestAction(CombatActor actor, List<CombatActor> participants);
    void SetAction(ActionContext ctx);
}
