using Unity.VisualScripting;

public class CombatCommandDispatcher
{
    private ActionSystem m_action;
    private ReactionSystem m_reaction;
    public CombatCommandDispatcher(ActionSystem action, ReactionSystem reaction) 
    {
        m_action = action;
        m_reaction = reaction;
    }
    public void Dispatch(ICombatCommand command) 
    {
        switch (command) 
        {
            case AttackCommand attackCmd:
                m_action.SubmitAction(attackCmd);
                break;
            case ReactionCommand reactionCmd:
                m_reaction.ReceiveReaction(reactionCmd);
                break;
        }
    }
}