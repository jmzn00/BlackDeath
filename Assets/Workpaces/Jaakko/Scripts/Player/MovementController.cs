using System;
using UnityEngine;
public class MovementController : MonoBehaviour, IActorComponent
{
    [Header("Movement")]
    [SerializeField] private float m_moveSpeed = 5f;

    private Rigidbody m_rigidBody;
    private InputManager m_inputManager;
    public event Action<Vector3> OnMove;

    public void LoadData(ActorSaveData data) 
    {
        transform.position = data.Position;
    }
    public void SaveData(ActorSaveData data) 
    {
        data.Position = transform.position;
    }

    public bool Initialize(GameManager game) 
    {
        // get input manager reference
        m_inputManager = game.Resolve<InputManager>();

        m_rigidBody = GetComponent<Rigidbody>();
        return true;
    }
    public bool Dispose() 
    {
        return true;
    }     
    private void FixedUpdate()
    {
        if (m_inputManager == null || m_rigidBody == null) return;

        var state = m_inputManager.GetInputState();
        Vector2 inputDir = state.InputDirection;
        Vector3 velocity = new Vector3(inputDir.x, 0f, inputDir.y) * m_moveSpeed;

        m_rigidBody.linearVelocity = velocity;
        OnMove?.Invoke(m_rigidBody.linearVelocity);
    }

}
