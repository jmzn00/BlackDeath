using UnityEngine;

public enum ReactionType 
{
    None,
    Parry,
    Dodge
}

public class ReactiveWindow
{
    public bool WindowOpen;
    private ReactionType m_currentReaction;
    
    public void Open() 
    {
        WindowOpen = true;
        m_currentReaction = ReactionType.None;
    }
    public void TryActivateParry() 
    {
        if (!WindowOpen) return;
        m_currentReaction = ReactionType.Parry;
    }
    public void TryActivateDodge() 
    {
        if (!WindowOpen) return;
        m_currentReaction = ReactionType.Dodge;
    }
    public ReactionType ConsumeReaction() 
    {

        ReactionType result = m_currentReaction;
        m_currentReaction = ReactionType.None;
        return result;
    }
    public void Reset() 
    {
        WindowOpen = false;
    }
    public void Update(float dt)
    {

    }
}
