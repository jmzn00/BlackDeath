using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Ai/ActionBehaviours/Villager/VillagerPattern")]
public class ActionBehaviour_VillagerPattern : AIActionBehaviour 
{    
    public override float Evaluate(CombatActor actor, List<CombatActor> participants
        , out CombatAction selectedAction)
    {
        selectedAction = null;

        AICombatActor ai = actor as AICombatActor;
        if (ai == null)
        {
            Debug.LogWarning("Actor is not AICombatActor on VillagerPattern");
            return -1f;
        }

        if (ai.Actions == null || ai.Actions.Count == 0)
            return -1f;

        int startIndex = ai.GetPatternIndex();
        int count = ai.Actions.Count;

        for (int i = 0; i < count; i++)
        {
            int index = (startIndex + i) % count;
            var action = ai.Actions[index];

            if (action is SkipTurnAction)
            {
                continue;
            }

            if (action.CanExecute(ai, out string reason))
            {
                selectedAction = action;

                ai.UpdatePatternIndex((index + 1) % count);

                return selectedAction.baseDamage;
            }
            else
            {
                Debug.Log($"AI cannot perform {action.actionName}: {reason}");
            }
        }

        selectedAction = ai.SkipAction;
        ai.UpdatePatternIndex((startIndex + 1) % count);

        return 0f;
    }
}