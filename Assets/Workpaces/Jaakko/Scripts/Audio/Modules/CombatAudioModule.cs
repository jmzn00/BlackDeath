using UnityEngine;

public class CombatAudioModule : AudioModuleBase
{
    private AudioSource m_source;
    private CombatSFXConfig m_config;

    public CombatAudioModule(AudioManager audio) : base(audio) { }

    public override void Activate()
    {
        base.Activate();
        m_source = m_audio.Controller.CombatSouce;
        m_config = m_audio.Controller.CombatSFX;

        CombatEvents.OnActionSubmitted        += OnTransitionStarted;
        CombatEvents.OnActionAnimationStarted += OnAttackStarted;
        CombatEvents.OnActionStrikeMoment     += OnStrikeMoment;
        CombatEvents.OnActionResolved         += OnActionResolved;
        CombatEvents.OnActorDied              += OnActorDied;
        CombatEvents.OnConfirmGraded          += OnConfirmGraded;
        CombatEvents.OnCombatStarted          += OnCombatStarted;
        CombatEvents.OnDodgeAttempted         += OnDodgeAttempted;
    }

    public override void Deactivate()
    {
        base.Deactivate();
        CombatEvents.OnActionSubmitted        -= OnTransitionStarted;
        CombatEvents.OnActionAnimationStarted -= OnAttackStarted;
        CombatEvents.OnActionStrikeMoment     -= OnStrikeMoment;
        CombatEvents.OnActionResolved         -= OnActionResolved;
        CombatEvents.OnActorDied              -= OnActorDied;
        CombatEvents.OnConfirmGraded          -= OnConfirmGraded;
        CombatEvents.OnCombatStarted          -= OnCombatStarted;
        CombatEvents.OnDodgeAttempted         -= OnDodgeAttempted;
    }

    public override void Update(float dt) { }

    private void OnTransitionStarted(ActionContext ctx)
    {
        if (ctx.Action.transitionSound != null)
            m_source.PlayOneShot(ctx.Action.transitionSound);
    }

    private void OnAttackStarted(ActionContext ctx)
    {
        if (ctx.Action.attackSound != null)
            m_source.PlayOneShot(ctx.Action.attackSound);
    }

    private void OnStrikeMoment(ActionContext ctx)
    {
        if (ctx.Action.strikeSound != null)
            m_source.PlayOneShot(ctx.Action.strikeSound);
    }

    private void OnActionResolved(ActionContext ctx, ActionResult res)
    {
        // Parry sound always plays on a successful parry
        if (res == ActionResult.Parried)
        {
            if (m_config?.parrySound != null)
                m_source.PlayOneShot(m_config.parrySound);
            return;
        }

        // Hit sound is a fallback — skip if the action has any custom audio
        bool actionHasAudio = ctx.Action.transitionSound != null
                           || ctx.Action.attackSound     != null
                           || ctx.Action.strikeSound     != null;
        if (actionHasAudio) return;
        if (m_config == null) return;

        AudioClip clip = res switch
        {
            ActionResult.Hit or ActionResult.Confirmed => m_config.hitSound,
            _                                          => null
        };
        if (clip != null) m_source.PlayOneShot(clip);
    }

    private void OnDodgeAttempted(ActionContext ctx)
    {
        if (m_config?.dodgeSound != null)
            m_source.PlayOneShot(m_config.dodgeSound);
    }

    private void OnActorDied(CombatActor actor)
    {
        if (m_config?.actorDeathSound != null)
            m_source.PlayOneShot(m_config.actorDeathSound);
    }

    private void OnConfirmGraded(ActionContext ctx, ConfirmGrade grade)
    {
        if (m_config == null) return;
        AudioClip clip = grade == ConfirmGrade.Perfect
            ? m_config.confirmPerfectSound
            : m_config.confirmGoodSound;
        if (clip != null) m_source.PlayOneShot(clip);
    }

    private void OnCombatStarted()
    {
        if (m_config?.combatStartSound != null)
            m_source.PlayOneShot(m_config.combatStartSound);
    }
}
