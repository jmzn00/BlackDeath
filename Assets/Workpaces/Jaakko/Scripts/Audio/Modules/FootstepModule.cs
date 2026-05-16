using UnityEngine;

public class FootstepModule : AudioModuleBase
{
    private AudioSource m_source;
    private FootstepBank m_bank;
    private float m_stepTimer;

    private bool m_transitionActive;

    private bool m_inExploration;
    private bool m_isMoving;
    private const float MovingThreshold = 0.5f;

    public FootstepModule(AudioManager audio) : base(audio) { }

    public override void Activate()
    {
        m_source = m_audio.Controller.CombatSouce;
        m_bank   = m_audio.Controller.HumanFootstepBank;
        m_active = true;

        CombatEvents.OnActionSubmitted += OnActionSubmitted;
        CombatEvents.OnTransitionEnded += OnTransitionEnded;
        GameEvents.OnPlayerMoved       += OnPlayerMoved;
    }

    public override void Deactivate()
    {
        // Intentionally empty — state managed via SetGameState
    }

    public void SetGameState(GameState state)
    {
        m_inExploration = state == GameState.None;

        if (!m_inExploration)
            m_isMoving = false;

        if (state != GameState.Combat)
        {
            m_transitionActive = false;
            m_stepTimer = 0f;
        }
    }

    private void OnActionSubmitted(ActionContext ctx)
    {
        if (!ctx.Action.useHumanFootsteps) return;
        m_transitionActive = true;
        m_stepTimer = 0f;
        PlayStep();
    }

    private void OnTransitionEnded()
    {
        m_transitionActive = false;
        m_stepTimer = 0f;
    }

    private void OnPlayerMoved(Vector3 velocity)
    {
        m_isMoving = velocity.magnitude > MovingThreshold;
    }

    public override void Update(float dt)
    {
        if (m_bank == null) return;

        bool shouldStep = m_transitionActive || (m_inExploration && m_isMoving);
        if (!shouldStep)
        {
            m_stepTimer = 0f;
            return;
        }

        m_stepTimer += dt;
        if (m_stepTimer >= m_bank.stepInterval)
        {
            m_stepTimer = 0f;
            PlayStep();
        }
    }

    private void PlayStep()
    {
        AudioClip clip = m_bank?.GetRandom();
        if (clip != null) m_source.PlayOneShot(clip);
    }
}
