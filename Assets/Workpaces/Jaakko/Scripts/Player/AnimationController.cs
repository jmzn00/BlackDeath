using System;
using UnityEngine;
public enum AnimationType 
{
    Parry,
    Dodge
}
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(MovementController))]
public class AnimationController : MonoBehaviour
{
    private Animator m_animator;
    private MovementController m_movementController;
    private CombatActor m_combatActor;
    [SerializeField] private SpriteRenderer m_spriteRenderer;

    [SerializeField] private AnimationClip m_idleAnim;
    [SerializeField] private AnimationClip m_runAnim;

    [SerializeField] private AnimationClip m_dodgeAnim;
    [SerializeField] private AnimationClip m_parryAnim;

    private void Awake()
    {
        m_animator = GetComponent<Animator>();
        m_movementController = GetComponent<MovementController>();

        m_movementController.OnMove += OnMove;
    }
    private void Start()
    {
        m_combatActor = GetComponent<CombatActor>();
        /*
        if (m_combatActor != null)
            m_combatActor.OnActionFinished += OnActionAnimationFinished;
        */
    }
    private void OnDestroy()
    {
        m_movementController.OnMove -= OnMove;
        /*
        if (m_combatActor != null)
            m_combatActor.OnActionFinished -= OnActionAnimationFinished;
        */
    }
    private void OnMove(Vector3 velocity) 
    {
        if (m_actionAnimationPlaying || m_defensiveAnimationPlaying)
            return;
        
        if (velocity.sqrMagnitude > 1f) 
        {
            m_animator.Play(m_runAnim.name);
        }
        else 
        {
            m_animator.Play(m_idleAnim.name);
        }

        if (velocity.x < 0)
            m_spriteRenderer.flipX = true;
        else
            m_spriteRenderer.flipX = false;
        
    }
    private bool m_actionAnimationPlaying;
    private bool m_defensiveAnimationPlaying;
    public void PlayActionAnimation(AnimationClip clip) 
    {
        m_actionAnimationPlaying = true;
        m_animator.Play(clip.name, 0, 0f);
    }
    public void PlayDefensiveAnimation(AnimationType type) 
    {
        if (m_defensiveAnimationPlaying) return;        

        AnimationClip clip = null;
        switch (type) 
        {
            case AnimationType.Parry:
                clip = m_parryAnim;
                break;
            case AnimationType.Dodge:
                clip = m_dodgeAnim;
                break;
        }
        if (clip == null) return;

        m_animator.Play(clip.name, 0, 0f);
        OnDefensiveAnimationPlaying?.Invoke(true);
        m_defensiveAnimationPlaying = true;
    }
    public event Action<bool> OnDefensiveAnimationPlaying;
    public void OnDefensiveAnimationFinished() 
    {
        m_defensiveAnimationPlaying = false;
        OnDefensiveAnimationPlaying?.Invoke(false);
        m_animator.Play(m_idleAnim.name, 0, 0f);
    }
    public void OnActionAnimationFinished() 
    {
        m_actionAnimationPlaying = false;
        m_combatActor.Anim_ActionFinished();
        m_animator.Play(m_idleAnim.name, 0, 0f);
    }
}
