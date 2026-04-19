using System;
using UnityEngine;

public class TransitionSystem : CombatSystemBase
{
    CombatArea m_area;
    ActionSystem m_action;
    CombatManager m_combat;
    public TransitionSystem(ActionSystem action
        , CombatManager combat) 
    {
        m_action = action;
        m_combat = combat;
    }
    public void UpdateArea(CombatArea area) 
    {
        m_area = area;
    }
    public override void Init(CombatContext context)
    {
        m_action.OnActionSubmitted += Start;
        m_action.OnActionFinished += ActionFinished;
    }
    public override void Reset() 
    {
        m_action.OnActionSubmitted -= Start;
        m_action.OnActionFinished -= ActionFinished;

        m_area = null;
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
    private void ActionFinished(ActionContext actx) 
    {
        ResetTransition();
    }
    public override void Update(float dt) 
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
    public void ResetTransition() 
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
        m_combat.ChangeState(CombatState.Transition);

        if (actx.Source == null) 
        {
            Debug.LogWarning($"Transition Source == NULL");
            Finish();
            return;
        }
        if (ShouldSkip(actx.Action.targetType))
        {
            Finish();
            return;
        }

        SetupActors(actx, out m_sourceActor, out m_targetActor);

        if (m_sourceActor == null) 
        {
            Finish();
            return;
        }
        m_sourceStart = m_sourceActor.position;
        if (m_targetActor != null)
            m_targetStart = m_targetActor.transform.position;

        bool isPlayer = actx.Source.Team == Team.Player;

        m_sourceEnd = isPlayer
            ? m_area.Preferences.m_partyActionPoint.position
            : m_area.Preferences.m_enemyActionPoint.position;
        if (m_targetActor != null) 
        {
            m_targetEnd = isPlayer 
                ? m_area.Preferences.m_enemyActionPoint.position
                : m_area.Preferences.m_partyActionPoint.position;
        }
        m_sourceDuration = GetDuration(actx.Source);
        m_targetDuration = GetDuration(actx.PrimaryTarget);
        
        PlayTransition(actx.Source);
        PlayTransition(actx.PrimaryTarget);

        m_transitionOpen = true;
        CombatEvents.TransitionStarted();
    }    
    private bool ShouldSkip(TargetType type) 
    {
        return type == TargetType.Self 
            || type == TargetType.Ally 
            || type == TargetType.AOEAlly;
    }
    private void SetupActors(ActionContext actx, out Transform sourceT, out Transform targetT) 
    {        
        sourceT = actx.Source.transform;
        targetT = null;

        if (actx.PrimaryTarget == null)
        {
            Debug.LogWarning($"SetupActors: PrimaryTarget == NULL");
            return;
        }

        switch (actx.Action.targetType) 
        {
            case TargetType.Enemy:
            case TargetType.AOEEnemy:
                targetT = actx.PrimaryTarget.transform;
                break;
            case TargetType.Self:
            case TargetType.Ally:
            case TargetType.AOEAlly:
                sourceT = null;
                targetT = null;
                break;
        }
    }
    private float GetDuration(CombatActor actor) 
    {
        if (actor == null) return DEFAULT_DURATION;

        return actor.HasTransition() && actor.TransitionClip
            ? actor.TransitionClip.length : DEFAULT_DURATION;
    }
    private void PlayTransition(CombatActor actor) 
    {
        if (actor == null) return;
        if (actor.HasTransition())
        {
            actor.PlayTransition();
        }
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
