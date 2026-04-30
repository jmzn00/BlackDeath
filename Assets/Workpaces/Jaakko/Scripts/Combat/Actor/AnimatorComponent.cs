using System;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimatorComponent : MonoBehaviour, IActorComponent
{
    public bool Initialize(GameManager game) { return true; }
    public bool Dispose()
    {
        m_combatActor.OnPlayRequested -= PlayCombatAction;

        if (m_movement != null)
            m_movement.OnMove -= Move;

        CombatEvents.OnCombatStarted -= CombatStarted;
        CombatEvents.OnCombatEnded -= CombatEnded;
        CombatEvents.OnTurnStarted -= TurnStarted;
        CombatEvents.OnTurnEnded -= TurnEnded;
        CombatEvents.OnTransitionEnded -= TransitionEnded;

        return true;
    }

    public void LoadData(ActorSaveData data) { }
    public void SaveData(ActorSaveData data) { }
    public void Load(object data)
    {

    }
    public object Save()
    {
        return null;
    }
    public void SetInputSource(IInputSource source) { }

    [Header("Animations")]
    [Header("Out Of Combat")]
    [SerializeField] private AnimationClip m_idle;
    [SerializeField] private AnimationClip m_walk;
    [Header("Combat")]
    [SerializeField] private AnimationClip m_combatIdle;
    [SerializeField] private AnimationClip m_parry;
    [SerializeField] private AnimationClip m_dodge;
    [SerializeField] private AnimationClip m_transition;

    public AnimationClip TransitionClip => m_transition;

    public event Action OnActionAnimationFinished;

    private CombatActor m_combatActor;
    private Animator m_animator;

    private MovementController m_movement;
    private bool isInCombat = false;
    public bool isInTurn = false;
    public void OnActorComponentsInitialized(Actor actor)
    {
        m_combatActor = actor.Get<CombatActor>();
        m_animator = GetComponent<Animator>();
        m_combatActor.OnPlayRequested += PlayCombatAction;

        CombatEvents.OnCombatStarted += CombatStarted;
        CombatEvents.OnCombatEnded += CombatEnded;
        CombatEvents.OnTurnStarted += TurnStarted;
        CombatEvents.OnTurnEnded += TurnEnded;
        CombatEvents.OnTransitionEnded += TransitionEnded;

        m_movement = actor.Get<MovementController>();
        if (m_movement != null)
            m_movement.OnMove += Move;
    }
    public bool IsAnimationPlaying(AnimationClip clip)
    {
        if (clip == null || m_animator == null)
            return false;

        AnimatorStateInfo stateInfo = m_animator.GetCurrentAnimatorStateInfo(0);

        return stateInfo.IsName(clip.name);
    }
    public void PlayParry()
    {
        if (!isInCombat) return;

        if (isInTurn) return;
        if (IsAnimationPlaying(m_parry))
            return;

        m_animator.Play(m_parry.name);
    }
    public void PlayDodge()
    {
        if (isInTurn) return;
        if (IsAnimationPlaying(m_dodge))
            return;
        if (!isInCombat) return;

        m_animator.Play(m_dodge.name);
    }
    private void TransitionEnded()
    {
        m_animator.Play(m_combatIdle.name, 0, 0f);
    }

    private void TurnEnded(CombatActor actor)
    {
        if (actor != m_combatActor) return;

        isInTurn = false;
        m_animator.Play(m_combatIdle.name, 0, 0f);
    }

    private void TurnStarted(CombatActor actor)
    {
        if (actor != m_combatActor) return;
        if (!isInCombat) isInCombat = true;
        isInTurn = true;
        m_animator.Play(m_combatIdle.name, 0, 0f);
    }

    private void Move(Vector3 vel)
    {
        if (isInCombat) return;

        if (vel.magnitude > 0.5f)
        {
            m_animator.Play(m_walk.name);
        }
        else
        {
            m_animator.Play(m_idle.name);
        }
    }
    private void CombatStarted()
    {
        isInCombat = true;
        m_animator.Play(m_combatIdle.name, 0, 0f);
    }
    private void CombatEnded(CombatResult result)
    {
        m_animator.Play(m_idle.name, 0, 0f);
        isInCombat = false;
    }
    // from m_combatActor.OnPlayRequested
    public void PlayCombatAction(AnimationClip clip)
    {
        m_animator.Play(clip.name, 0, 0f);
    }
    public void PlayTransition()
    {
        m_animator.Play(m_transition.name, 0, 0f);
    }
    // called by animation
    public void Anim_OpenWindow(string promptKey)
    {
        m_combatActor.OpenWindow(promptKey);
    }
    // called by animation
    public void Anim_CloseWindow()
    {
        if (m_combatActor == null)
        {
            Debug.LogWarning("Combat Actor is NULL on AnimatiorComponent");
            return;
        }

        m_combatActor.CloseWindow();

    }
    // called by animation
    public void Anim_Finished()
    {
        if (m_combatActor == null)
        {
            Debug.LogWarning("Combat Actor is NULL on AnimatiorComponent");
            return;
        }
        m_animator.Play(m_combatIdle.name, 0, 0f);
        OnActionAnimationFinished?.Invoke();
    }

    public bool ParryOpen { get; private set; }
    public bool DodgeOpen { get; private set; }
    public void Anim_OpenParry()
    {
        ParryOpen = true;
    }
    public void Anim_CloseParry()
    {
        ParryOpen = false;
    }
    public void Anim_OpenDodge()
    {
        Debug.Log("Open Dodge");
        DodgeOpen = true;
    }
    public void Anim_CloseDodge()
    {
        Debug.Log("Close Dodge");
        DodgeOpen = false;
    }
}