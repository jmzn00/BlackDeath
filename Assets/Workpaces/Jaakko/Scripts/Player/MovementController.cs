using System;
using UnityEngine;

[RequireComponent(typeof(Actor))]
public class MovementController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float m_moveSpeed = 5f;

    private Actor m_actor;

    private Rigidbody m_rigidBody;

    private InputManager m_inputManager;

    public event Action<Vector3> OnMove;

    private void Awake()
    {
        m_actor = GetComponent<Actor>();
        m_actor.OnActorInit += OnActorEnable;

        m_rigidBody = GetComponent<Rigidbody>();
    }
    private void OnDestroy()
    {
        m_actor.OnActorInit -= OnActorEnable;
    }
    private void OnActorEnable(bool value) 
    {
        m_inputManager = Services.Get<InputManager>();
    }
    private void FixedUpdate()
    {
        var state = m_inputManager.GetInputState();

        Vector2 inputDir = state.InputDirection;

        Vector3 velocity = new Vector3(inputDir.x, 0f, inputDir.y) * m_moveSpeed;

        m_rigidBody.linearVelocity = velocity;
        OnMove?.Invoke(m_rigidBody.linearVelocity);
    }

}
