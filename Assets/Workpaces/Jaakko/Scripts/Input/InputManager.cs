using UnityEngine;
public struct InputState 
{
    public bool JumpPressed;
    public bool JumpHeld;
   
    public Vector2 InputDirection;

    public bool DashPressed;
    public bool DashPressedThisFrame;
    public bool CrouchPressed;
}
public class InputManager : IManager
{
    private bool m_active;
    private InputState m_inputState;
    private InputSystem_Actions m_inputActions;
    public InputSystem_Actions InputActions => m_inputActions;

    public InputManager() 
    {
        
    }
    public ref InputState GetInputState() 
    {
        return ref m_inputState;
    }
    public void Update(float dt) 
    {
        if (!m_active) return;

        m_inputState.InputDirection =
            m_inputActions.Player.Move.ReadValue<Vector2>();
        m_inputState.JumpPressed =
            m_inputActions.Player.Jump.WasPressedThisFrame();
        m_inputState.JumpHeld =
            m_inputActions.Player.Jump.IsPressed();

        m_inputState.DashPressedThisFrame = 
            m_inputActions.Player.Dash.WasPressedThisFrame();

        m_inputState.DashPressed = 
            m_inputActions.Player.Dash.IsPressed();

        m_inputState.CrouchPressed =
            m_inputActions.Player.Crouch.IsPressed();
    }
    public void OnManagersInitialzied()
    {

    }
    public bool Init()
    {
        m_inputActions = new InputSystem_Actions();
        m_inputActions.Enable();
        m_active = true;        
        
        return true;    
    }
    public bool Dispose()
    {
        m_active = false;

        m_inputActions.Disable();
        m_inputActions.Dispose();        
        return true;
    }   
}
