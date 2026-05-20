using System;
using UnityEngine;

public struct MovmentState
{
    public bool IsGrounded;
    public Vector3 ContactNormal;
    public bool OnWall;
    public bool IsSliding;
}

[RequireComponent(typeof(CharacterController))]
public class MovementController : MonoBehaviour, IActorComponent
{
    [Header("Stats")]
    [SerializeField] private PlayerStats m_playerStats;

    [Header("Visuals")]
    [SerializeField] private GameObject m_visualCapsule;

    private CharacterController m_controller;
    private IInputSource m_inputSource;
    private InputState m_inputState;
    private Vector3 m_velocity;
    private bool m_isGrounded;
    private bool m_wasRunning;

    public Vector3 Velocity => m_velocity;
    public InputState InputState => m_inputState;
    public MovmentState MovmentState => new MovmentState { IsGrounded = m_isGrounded, ContactNormal = Vector3.up };
    public PlayerStats RuntimeStats => m_playerStats;
    public Transform Player => transform;

    public event Action<Vector3> OnMove;

    // ── IActorComponent ──────────────────────────────────────────────

    public bool Initialize(GameManager game)
    {
        m_controller = GetComponent<CharacterController>();
        m_controller.slopeLimit = 45f;
        m_controller.stepOffset = 0.35f;
        CombatEvents.OnCombatStarted += FaceRight;
        return true;
    }

    public void OnActorComponentsInitialized(Actor actor) { }

    public bool Dispose()
    {
        CombatEvents.OnCombatStarted -= FaceRight;
        return true;
    }

    private void FaceRight()
    {
        if (m_visualCapsule != null)
            m_visualCapsule.transform.rotation = Quaternion.Euler(0, 180, 0);
    }
    public void SaveData(ActorSaveData data) { }

    public void LoadData(ActorSaveData data)
    {
        m_controller.enabled = false;
        transform.position = data.Position;
        m_controller.enabled = true;
    }

    // ── Public API ───────────────────────────────────────────────────

    public void SetInputSource(IInputSource source) => m_inputSource = source;

    public void Move(Vector3 pos)
    {
        m_controller.enabled = false;
        m_velocity = Vector3.zero;
        transform.position = pos;
        m_controller.enabled = true;
    }

    public void MoveTo(Transform t)
    {
        m_controller.enabled = false;
        transform.position = t.position;
        transform.rotation = t.rotation;
        m_controller.enabled = true;
    }

    // ── Update ───────────────────────────────────────────────────────

    private void Update()
    {
        if (m_inputSource == null) return;

        m_inputState = m_inputSource.GetInput();
        m_isGrounded = m_controller.isGrounded;

        UpdateVisuals();
        ApplyGravity();
        ApplyMovement();

        m_controller.Move(m_velocity * Time.deltaTime);
        Vector3 horizontalVelocity = new Vector3(m_velocity.x, 0f, m_velocity.z);
        OnMove?.Invoke(horizontalVelocity);
        GameEvents.PlayerMoved(horizontalVelocity);
    }

    private void UpdateVisuals()
    {
        if (m_visualCapsule == null) return;
        if (m_inputState.InputDirection.x > 0.1f)
            m_visualCapsule.transform.rotation = Quaternion.Euler(0, 180, 0);
        else if (m_inputState.InputDirection.x < -0.1f)
            m_visualCapsule.transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    private void ApplyGravity()
    {
        if (m_isGrounded && m_velocity.y < 0f)
            m_velocity.y = -2f;
        else
            m_velocity.y += m_playerStats.Gravity * Time.deltaTime;
    }

    private void ApplyMovement()
    {
        Vector2 input = m_inputState.InputDirection;
        if (input.sqrMagnitude > 1f) input.Normalize();

        bool isRunning = m_inputState.RunHeld && input.sqrMagnitude > 0.01f;
        if (isRunning != m_wasRunning)
        {
            GameEvents.PlayerRunChanged(isRunning);
            m_wasRunning = isRunning;
        }

        float targetSpeed = isRunning ? m_playerStats.RunSpeed : m_playerStats.WalkSpeed;
        Vector3 wish = (transform.forward * input.y + transform.right * input.x) * targetSpeed;

        float rate = input.sqrMagnitude > 0.01f ? m_playerStats.Acceleration : m_playerStats.Deceleration;
        m_velocity.x = Mathf.MoveTowards(m_velocity.x, wish.x, rate * Time.deltaTime);
        m_velocity.z = Mathf.MoveTowards(m_velocity.z, wish.z, rate * Time.deltaTime);
    }
}
