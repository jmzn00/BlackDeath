using System.Runtime.InteropServices;

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

    private float m_parryWindow = 0.4f;
    private float m_dodgeWindow = 0.7f;

    private float m_parryCooldownTimer;
    private float m_dodgeCooldownTimer;

    private float m_parryCooldown = 0.1f;
    private float m_dodgeCooldown = 0.1f;

    private float m_time;

    public bool IsOpen => m_windowOpen;
    public float Time => m_time;
    public bool CanParry => m_windowOpen
        && m_time <= m_parryWindow;
    public bool CanDodge => m_windowOpen && m_time <= m_dodgeWindow;
    
    public void Open() 
    {
        m_windowOpen = true;
        m_currentReaction = ReactionType.None;
    }
    public void TryActivateParry() 
    {
        if (!CanParry) return;

        if (m_parryCooldownTimer > 0f) return;
        if (m_currentReaction != ReactionType.None) return;

        m_currentReaction = ReactionType.Parry;
        m_parryCooldownTimer = m_parryCooldown;
    }
    public void TryActivateDodge() 
    {
        if (!CanDodge) return;

        if (m_dodgeCooldownTimer > 0f) return;
        if (m_currentReaction != ReactionType.None) return;

        m_currentReaction = ReactionType.Dodge;
        m_dodgeCooldownTimer = m_dodgeCooldown;
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
        if (m_parryCooldownTimer > 0f)
            m_parryCooldownTimer -= dt;
        if (m_dodgeCooldownTimer > 0f)
            m_dodgeCooldownTimer -= dt;

        if (!m_windowOpen) return;
        m_time += dt;
    }
}
