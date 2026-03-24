using System;
using System.Collections.Generic;
public interface IActionProvider
{
    void RequestAction(CombatActor actor, List<CombatActor> participants);
    void SetAction(ActionContext ctx);

    event Action<AttackCommand> OnCommandReady;
    void Begin(CombatActor actor, List<CombatActor> participants);
    ICombatCommand GetCommand();
}
