using System;
using UnityEngine;

public class TransitionSystem : CombatSystemBase
{
    CombatArea m_area;
    ActionSystem m_action;
    CombatManager m_combat;

    public TransitionSystem(ActionSystem action, CombatManager combat)
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

    // Forward transition
    private Transform m_sourceActor;
    private Transform m_targetActor;

    private Vector3 m_sourceStart;
    private Vector3 m_targetStart;
    private Vector3 m_sourceEnd;

    private float m_sourceTime;
    private float m_sourceDuration;
    private const float DEFAULT_DURATION = 3f;

    private bool m_transitionOpen;

    // Return transition
    private bool m_returningToStart;
    private float m_returnTime;
    private Vector3 m_returnStartPos;
    private const float RETURN_DURATION = 0.55f;

    private void ActionFinished(ActionContext actx)
    {
        if (m_sourceActor == null) return;

        m_returnStartPos = m_sourceActor.position;
        m_returnTime = 0f;
        m_returningToStart = true;
    }

    public override void Update(float dt)
    {
        if (m_transitionOpen)
        {
            m_sourceTime += dt;
            float sourceT = Mathf.Clamp01(m_sourceTime / m_sourceDuration);

            if (m_sourceActor != null)
                m_sourceActor.position = Vector3.Lerp(m_sourceStart, m_sourceEnd, sourceT);

            if (sourceT >= 1f)
                Finish();
        }

        if (m_returningToStart)
        {
            m_returnTime += dt;
            float t = Mathf.Clamp01(m_returnTime / RETURN_DURATION);

            if (m_sourceActor != null)
                m_sourceActor.position = Vector3.Lerp(m_returnStartPos, m_sourceStart, t);

            if (t >= 1f)
                FinishReturn();
        }
    }

    public void ResetTransition()
    {
        if (m_sourceActor != null) m_sourceActor.position = m_sourceStart;
        if (m_targetActor != null) m_targetActor.position = m_targetStart;

        m_sourceActor = null;
        m_targetActor = null;
        m_transitionOpen = false;
        m_returningToStart = false;
    }

    public void Start(ActionContext actx)
    {
        m_combat.ChangeState(CombatState.Transition);

        if (actx.Source == null)
        {
            Debug.LogWarning("Transition Source == NULL");
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
            m_targetStart = m_targetActor.position;

        // Move attacker to a point directly beside the target.
        // Offset only on the dominant separation axis so the attacker
        // snaps to the target's position on the other axis.
        if (m_targetActor != null)
        {
            Vector3 toSource = m_sourceStart - m_targetActor.position;
            toSource.y = 0f;
            float offset = m_area != null ? m_area.Preferences.m_attackOffset : 1.5f;

            Vector3 attackPos = m_targetActor.position;
            attackPos.y = m_sourceStart.y;

            if (Mathf.Abs(toSource.x) >= Mathf.Abs(toSource.z))
                attackPos.x += Mathf.Sign(toSource.x) * offset;
            else
                attackPos.z += Mathf.Sign(toSource.z) * offset;

            m_sourceEnd = attackPos;
        }
        else
        {
            m_sourceEnd = m_sourceStart;
        }

        m_sourceDuration = GetDuration(actx.Source);
        m_sourceTime = 0f;

        // Only the attacker plays the transition animation
        PlayTransition(actx.Source);

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
            Debug.LogWarning("SetupActors: PrimaryTarget == NULL");
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
        return actor.HasTransition() && actor.TransitionClip ? actor.TransitionClip.length : DEFAULT_DURATION;
    }

    private void PlayTransition(CombatActor actor)
    {
        if (actor == null) return;
        if (actor.HasTransition())
            actor.PlayTransition();
    }

    private void Finish()
    {
        m_transitionOpen = false;
        m_sourceTime = 0f;

        OnTransitionFinished?.Invoke();
        CombatEvents.TransitionEnded();
    }

    private void FinishReturn()
    {
        if (m_sourceActor != null)
            m_sourceActor.position = m_sourceStart;

        m_sourceActor = null;
        m_targetActor = null;
        m_returningToStart = false;
    }
}
