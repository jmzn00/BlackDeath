using System.Net.NetworkInformation;
using Unity.VisualScripting;
using UnityEngine;

public enum ReactionType 
{
    None,
    Parry,
    Dodge
}

public class ReactiveWindow
{
    private bool m_windowOpen = false;
    private ReactionType m_currentReaction;

    private float m_parryWindow = 0.2f;
    private float m_dodgeWindow = 0.5f;

    private float m_time;

    public bool IsOpen => m_windowOpen;
    public float Time => m_time;
    public bool CanParry => m_windowOpen && m_time <= m_parryWindow;
    public bool CanDodge => m_windowOpen && m_time <= m_dodgeWindow;
    
    public void Open() 
    {
        m_windowOpen = true;
        m_currentReaction = ReactionType.None;
    }
    public void TryActivateParry() 
    {
        if (!m_windowOpen
            || m_time > m_parryWindow) return;

        m_currentReaction = ReactionType.Parry;
    }
    public void TryActivateDodge() 
    {
        if (!m_windowOpen
            || m_time > m_dodgeWindow) return;

        m_currentReaction = ReactionType.Dodge;
    }
    public ReactionType ConsumeReaction() 
    {
        ReactionType result = m_currentReaction;
        m_currentReaction = ReactionType.None;
        m_time = 0f;        
        return result;
    }
    public void Reset() 
    {
        m_windowOpen = false;
    }
    public void Update(float dt)
    {
        if (!m_windowOpen) return;

        m_time += dt;
    }
}
