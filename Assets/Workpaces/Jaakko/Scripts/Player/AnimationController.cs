using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(MovementController))]
public class AnimationController : MonoBehaviour
{
    private Animator m_animator;
    private MovementController m_movementController;
    [SerializeField] private SpriteRenderer m_spriteRenderer;

    [SerializeField] private AnimationClip m_idleAnim;
    [SerializeField] private AnimationClip m_runAnim;

    private void Awake()
    {
        m_animator = GetComponent<Animator>();
        m_movementController = GetComponent<MovementController>();

        m_movementController.OnMove += OnMove;
    }
    private void OnDestroy()
    {
        m_movementController.OnMove -= OnMove;
    }
    private void OnMove(Vector3 velocity) 
    {
        if (m_actionAnimationPlaying)
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
    public void PlayActionAnimation(AnimationClip clip) 
    {
        m_actionAnimationPlaying = true;
        m_animator.Play(clip.name, 0, 0f);
    }
    public void OnActionAnimationFinished() 
    {
        m_actionAnimationPlaying = false;

        m_animator.Play(m_idleAnim.name, 0, 0f);
    }
}
