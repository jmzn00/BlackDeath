using System;
using UnityEngine;

public class TransitionSystem
{
    CombatArea m_area;
    public TransitionSystem(CombatArea area) 
    {
        m_area = area;
        transitionOpen = false;
    }
    public event Action OnTransitionFinished;

    private Transform m_sourceActorPoint;
    private Transform m_targetActorPoint;

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

    private bool transitionOpen;
    public void Update(float dt) 
    {
        if (!transitionOpen) return;

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

        transitionOpen = false;
    }
    public void Start(ActionContext actx) 
    {
        m_sourceActor = actx.Source.transform;
        m_targetActor = actx.Target.transform;

        m_sourceStart = m_sourceActor.position;
        m_targetStart = m_targetActor.position;

        if (actx.Source.IsPlayer) 
        {
            m_sourceEnd = m_area.Preferences.m_partyActionPoint.position;
            m_targetEnd = m_area.Preferences.m_enemyActionPoint.position;
        }
        else 
        {
            m_sourceEnd = m_area.Preferences.m_enemyActionPoint.position;
            m_targetEnd = m_area.Preferences.m_partyActionPoint.position;           
        }

        m_sourceDuration = actx.Source.HasTransition() && actx.Source.TransitionClip
            ? actx.Source.TransitionClip.length : DEFAULT_DURATION;
        m_targetDuration = actx.Target.HasTransition() && actx.Target.TransitionClip
            ? actx.Target.TransitionClip.length : DEFAULT_DURATION;

        transitionOpen = true;
    }    
    private void Finish() 
    {
        transitionOpen = false;
        m_sourceTime = 0f;
        m_targetTime = 0f;

        OnTransitionFinished?.Invoke();
    }
}
