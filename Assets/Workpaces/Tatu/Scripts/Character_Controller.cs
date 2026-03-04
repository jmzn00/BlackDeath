
using UnityEngine;

public class Character_Controller : MonoBehaviour
{
    private InputManager m_inputManager;
    private InputSystem_Actions m_inputActions;

    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Animations")]
    [SerializeField] AnimationClip idleAnim;
    [SerializeField] AnimationClip runAnim;
    [SerializeField] SpriteRenderer spriteRenderer;
    private Animator m_animator;

    // Rigidbody for physics-based movement
    private Rigidbody m_rigidbody;

    // Current input direction (x = horizontal, y = vertical -> maps to x,z world)
    private Vector2 m_moveDirection = Vector2.zero;

    private void Awake()
    {
        m_animator = GetComponent<Animator>();
        m_rigidbody = GetComponent<Rigidbody>();

        // Try to acquire input service (may be null at first; Update will retry)
        m_inputManager = Services.Get<InputManager>();
        if (m_inputManager != null)
            m_inputActions = m_inputManager.InputActions;

        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        // Try to lazy-acquire the input manager if not ready yet
        if (m_inputManager == null)
        {
            m_inputManager = Services.Get<InputManager>();
            if (m_inputManager != null)
                m_inputActions = m_inputManager.InputActions;
        }

        // Read input safely; keep last known direction if input manager not available
        if (m_inputManager != null)
        {
            m_moveDirection = m_inputManager.GetInputState().InputDirection;
        }
        else
        {
            m_moveDirection = Vector2.zero;
        }

        // Animation and sprite flip logic runs on Update (visuals only)
        if (m_moveDirection.magnitude > 0f)
        {
            if (m_animator != null && runAnim != null)
                m_animator.Play(runAnim.name);
        }
        else
        {
            if (m_animator != null && idleAnim != null)
                m_animator.Play(idleAnim.name);
        }

        if (spriteRenderer != null)
        {
            if (m_moveDirection.x > 0f)
                spriteRenderer.flipX = false;
            else if (m_moveDirection.x < 0f)
                spriteRenderer.flipX = true;
        }
    }

    private void FixedUpdate()
    {
        // Physics-driven movement using Rigidbody
        if (m_rigidbody == null)
            return;

        // desired velocity in world space (x, 0, y)
        Vector3 desiredVelocity = new Vector3(m_moveDirection.x, 0f, m_moveDirection.y) * moveSpeed;

        if (m_rigidbody.isKinematic)
        {
            // For kinematic rigidbodies use MovePosition
            Vector3 nextPos = m_rigidbody.position + desiredVelocity * Time.fixedDeltaTime;
            m_rigidbody.MovePosition(nextPos);
        }
        else
        {
            // For dynamic rigidbodies set velocity to desired value (preserves physics interactions)
            // Keep current y-velocity (gravity) intact
            Vector3 vel = m_rigidbody.linearVelocity;
            vel.x = desiredVelocity.x;
            vel.z = desiredVelocity.z;
            m_rigidbody.linearVelocity = vel;
        }
    }
}