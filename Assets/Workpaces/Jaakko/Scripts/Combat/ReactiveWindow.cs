using UnityEngine;

public class ReactiveWindow
{
    public bool ParryActive { get; private set; }
    public bool DodgeActive { get; private set; }

    private float m_parryEndTime;
    private float m_dodgeEndTime;

    private float m_parryCooldownEnd;
    private float m_dodgeCooldownEnd;

    private float m_time;

    private readonly float m_parryDuration = 0.2f;
    private readonly float m_dodgeDuration = 0.4f;
    private readonly float m_parryCooldown = 1f;
    private readonly float m_dodgeCooldown = 0.5f;

    public void Update(float dt) 
    {
        m_time += dt;

        if (ParryActive && m_time >= m_parryCooldownEnd)
            ParryActive = false;

        if (DodgeActive && m_time >= m_dodgeEndTime)
            DodgeActive = false;
    }

    public void TryActivateParry() 
    {
        if (m_time < m_parryCooldownEnd) return;

        ParryActive = true;
        m_parryEndTime = m_time + m_parryDuration;
        m_parryCooldownEnd = m_time + m_parryCooldown;
    }
    public void TryActivateDodge() 
    {
        if (m_time < m_dodgeCooldownEnd) return;

        DodgeActive = true;
        m_dodgeEndTime = m_time + m_dodgeDuration;
        m_dodgeCooldownEnd = m_time + m_dodgeCooldown;
    }
    public void Reset() 
    {
        ParryActive = false;
        DodgeActive = false;        
    }
}
