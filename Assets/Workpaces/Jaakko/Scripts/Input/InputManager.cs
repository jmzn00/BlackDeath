using UnityEngine;

public struct InputState 
{
    public bool JumpPressed;
    public Vector2 InputDirection;
}

public class InputManager : MonoBehaviour
{
    private InputState m_inputState;
    public InputState InputState => m_inputState;

    private InputSystem_Actions m_inputActions;
    public InputSystem_Actions InputActions => m_inputActions;

    public static InputManager I;

    private void Awake()
    {
        if (I != this)  return;
        I = this;
        m_inputActions = new InputSystem_Actions();
        m_inputActions.Enable();

        BindInputs();
    }
    private void OnDestroy()
    {
        if (I == this) I = null;

        m_inputActions.Disable();
        m_inputActions.Dispose();
    }

    private void BindInputs() 
    {
        m_inputActions.Player.Move.performed += ctx =>
            m_inputState.InputDirection = ctx.ReadValue<Vector2>();
       m_inputActions.Player.Move.canceled += ctx =>
            m_inputState.InputDirection = Vector2.zero;

        m_inputActions.Player.Jump.performed += ctx =>
            m_inputState.JumpPressed = true;
        m_inputActions.Player.Jump.canceled += ctx =>
            m_inputState.JumpPressed = false;


    }
}
