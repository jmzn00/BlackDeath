using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Ai/ActionBehaviours/Villager/VillagerPattern")]
public class ActionBehaviour_VillagerPattern : AIActionBehaviour 
{    
    public override float Evaluate(CombatActor actor, List<CombatActor> participants
        , out CombatAction selectedAction)
    {
        selectedAction = null;

        if (actor.Actions == null || actor.Actions.Count == 0)
            return -1f;

        int startIndex = actor.GetPatternIndex();
        int count = actor.Actions.Count;
        for (int i = 0; i < count; i++)
        {
            int index = (startIndex + i) % count;
            var action = actor.Actions[index];

            if (action == actor.SkipAction)
            {
                continue;
            }

            if (action.CanExecute(actor, out string reason))
            {
                selectedAction = action;

                actor.UpdatePatternIndex((index + 1) % count);

                return selectedAction.baseDamage;
            }
            else
            {
                Debug.Log($"AI cannot perform action: {action.actionName}. reason: {reason}");
            }
        }

        selectedAction = actor.SkipAction;
        actor.UpdatePatternIndex((startIndex + 1) % count);

        return 0f;
    }
}