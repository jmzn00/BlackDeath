using System;
using UnityEngine;

public class TransitionSystem
{
    CombatArea m_area;
    public TransitionSystem(CombatArea area) 
    {
        m_area = area;
    }    
    public event Action OnTransitionFinished;

    private Transform m_sourceActor;
    private Transform m_targetActor;

    private Vector3 m_sourceStart;
    private Vector3 m_targetStart;

    private Vector3 m_sourceEnd;
    private Vector3 m_targetEnd;

    private float m_sourceTime;
    private float m_targetTime;

    private float m_sourceDuration;
    private float m_targetDuration;
    private const float DEFAULT_DURATION = 3f;

    private bool m_transitionOpen;
    
    public void Update(float dt) 
    {
        if (!m_transitionOpen) return;

        m_sourceTime += dt;
        m_targetTime += dt;

        float sourceT = Mathf.Clamp01(m_sourceTime / m_sourceDuration);
        float targetT = Mathf.Clamp01(m_targetTime / m_targetDuration);

        if (m_sourceActor != null)
            m_sourceActor.position = Vector3.Lerp(m_sourceStart, m_sourceEnd, sourceT);
        if (m_targetActor != null)
            m_targetActor.position = Vector3.Lerp(m_targetStart, m_targetEnd, targetT);

        if (sourceT >= 1f && targetT >= 1f) 
        {
            Finish();
        }        
    }    
    public void Reset() 
    {
        if (m_sourceActor != null)
            m_sourceActor.position = m_sourceStart;
        if (m_targetActor != null)
            m_targetActor.position = m_targetStart;

        m_sourceActor = null;
        m_targetActor = null;

        m_transitionOpen = false;
    }
    public void Start(ActionContext actx) 
    {
        CombatActor source = actx.Source;
        CombatActor target = actx.Target;

        m_sourceActor = source.transform;
        m_targetActor = target.transform;

        m_sourceStart = m_sourceActor.position;
        m_targetStart = m_targetActor.position;

        if (source.IsPlayer) 
        {
            m_sourceEnd = m_area.Preferences.m_partyActionPoint.position;
            m_targetEnd = m_area.Preferences.m_enemyActionPoint.position;
        }
        else 
        {
            m_sourceEnd = m_area.Preferences.m_enemyActionPoint.position;
            m_targetEnd = m_area.Preferences.m_partyActionPoint.position;           
        }                
        m_sourceDuration = source.HasTransition() && source.TransitionClip
            ? source.TransitionClip.length : DEFAULT_DURATION;
        m_targetDuration = target.HasTransition() && target.TransitionClip
            ? target.TransitionClip.length : DEFAULT_DURATION;

        if (source.HasTransition())
        {
            source.PlayTransition();
        }
        if (target.HasTransition())
        {
            target.PlayTransition();
        }

        m_transitionOpen = true;
        CombatEvents.TransitionStarted();
    }    
    private void Finish() 
    {
        m_transitionOpen = false;
        m_sourceTime = 0f;
        m_targetTime = 0f;

        OnTransitionFinished?.Invoke();
        CombatEvents.TransitionEnded();
    }
}
