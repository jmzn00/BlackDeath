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
        if (velocity.magnitude > 0.001f) 
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
}
