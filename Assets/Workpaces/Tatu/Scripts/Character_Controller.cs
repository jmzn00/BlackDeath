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


    private void Awake()
    {
        m_animator = GetComponent<Animator>();
        m_inputManager = Services.Get<InputManager>();
        m_inputActions = m_inputManager.InputActions;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {

        var moveDirection = m_inputManager.GetInputState().InputDirection;
        // Move the character in x and z directions based on input
        Vector3 movement = new Vector3(moveDirection.x, 0, moveDirection.y) * moveSpeed * Time.deltaTime;
        transform.Translate(movement, Space.World);

        if (moveDirection.magnitude > 0)
        {
            m_animator.Play(runAnim.name);
        }
        else
        {
            m_animator.Play(idleAnim.name);
        }

        if (moveDirection.x > 0)
            spriteRenderer.flipX = false;
        else if (moveDirection.x < 0)
            spriteRenderer.flipX = true;
    }
}
