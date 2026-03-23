using System;
using UnityEngine;
public enum UIInputAction
{
    Submit,
    Cancel
}
public struct InputState 
{
    public bool JumpPressed;
    public bool JumpHeld;
   
    public Vector2 InputDirection;

    public bool DashPressed;
    public bool DashPressedThisFrame;
    public bool CrouchPressed;

    public bool ParryPressed;
    public bool DodgePressed;

    public bool ParryPressedThisFrame;
    public bool DodgePressedThisFrame;

    public bool InteractPressedThisFrame;

    public bool MouseDownPressedThisFrame;
}
public struct UIInputState 
{
    public Vector2 InputDirection;

    public bool NavigateUpPressed;
    public bool NavigateDownPressed;
    public bool NavigateLeftPressed;
    public bool NavigateRightPressed;

    internal bool _verticalUsedLastFrame;
    internal bool _horizontalUsedLastFrame;

}
public class InputManager : IManager
{
    private bool m_active;
    private InputState m_inputState;
    private UIInputState m_uiInputState;

    private InputSystem_Actions m_inputActions;
    public InputSystem_Actions InputActions => m_inputActions;

    public event Action<UIInputAction> OnUIInputAction;
    public event Action<float> OnSelectTarget;

    public InputManager() 
    {
        
    }
    public ref InputState GetInputState() 
    {
        return ref m_inputState;
    }
    public ref UIInputState GetUIInputState() 
    {
        return ref m_uiInputState;
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

        m_inputState.InteractPressedThisFrame =
            m_inputActions.Player.Interact.WasPressedThisFrame();

        m_inputState.ParryPressed = 
            m_inputActions.Combat.Parry.IsPressed();

        m_inputState.DodgePressed = 
            m_inputActions.Combat.Dodge.IsPressed();

        m_inputState.ParryPressedThisFrame =
            m_inputActions.Combat.Parry.WasPressedThisFrame();

        m_inputState.DodgePressedThisFrame =
            m_inputActions.Combat.Dodge.WasPressedThisFrame();


        if (m_inputActions.UI.Submit.WasPressedThisFrame())
            OnUIInputAction?.Invoke(UIInputAction.Submit);
        if (m_inputActions.UI.Cancel.WasPressedThisFrame())
            OnUIInputAction?.Invoke(UIInputAction.Cancel);

        HandleUIInput();


    }
    private void HandleUIInput() 
    {
        Vector2 raw = m_inputActions.UI.Navigate.ReadValue<Vector2>();
        m_uiInputState.InputDirection = raw;

        if (raw.y > 0.5f)
        {
            m_uiInputState.NavigateUpPressed = !m_uiInputState._verticalUsedLastFrame;
            m_uiInputState.NavigateDownPressed = false;
            m_uiInputState._verticalUsedLastFrame = true;
        }
        else if (raw.y < -0.5f)
        {
            m_uiInputState.NavigateDownPressed = !m_uiInputState._verticalUsedLastFrame;
            m_uiInputState.NavigateUpPressed = false;
            m_uiInputState._verticalUsedLastFrame = true;
        }
        else
        {
            m_uiInputState.NavigateUpPressed = false;
            m_uiInputState.NavigateDownPressed = false;
            m_uiInputState._verticalUsedLastFrame = false;
        }

        if (raw.x > 0.5f)
        {
            m_uiInputState.NavigateRightPressed = !m_uiInputState._horizontalUsedLastFrame;
            m_uiInputState.NavigateLeftPressed = false;
            m_uiInputState._horizontalUsedLastFrame = true;
        }
        else if (raw.x < -0.5f)
        {
            m_uiInputState.NavigateLeftPressed = !m_uiInputState._horizontalUsedLastFrame;
            m_uiInputState.NavigateRightPressed = false;
            m_uiInputState._horizontalUsedLastFrame = true;
        }
        else
        {
            m_uiInputState.NavigateRightPressed = false;
            m_uiInputState.NavigateLeftPressed = false;
            m_uiInputState._horizontalUsedLastFrame = false;
        }



        float targetAxis = m_inputActions.Combat.SelectTarget.ReadValue<float>();

        if (targetAxis > 0.5f) 
        {
            if (!m_selectTargetUsedLastFrame) 
            {
                OnSelectTarget?.Invoke(1f);
                m_selectTargetUsedLastFrame = true;
            }
        }
        else if (targetAxis < -0.5f) 
        {
            if (!m_selectTargetUsedLastFrame) 
            {
                OnSelectTarget?.Invoke(-1f);
                m_selectTargetUsedLastFrame = true;
            }
        }
        else 
        {
            m_selectTargetUsedLastFrame = false;
        }
    }
    private bool m_selectTargetUsedLastFrame = false;
    public void OnManagersInitialzied()
    {

    }
    private void CombatStarted()
    {
        ToggleInput(false);
    }
    private void CombatEnded(CombatResult result)
    {
        ToggleInput(true);
    }
    public void ToggleInput(bool value) 
    {
        if (!value) 
        {
            m_inputState = new InputState();
            m_inputActions.Player.Disable();
        }
        else 
        {
            m_inputActions.Player.Enable();
        }
    }
    public bool Init()
    {
        CombatEvents.OnCombatStarted += CombatStarted;
        CombatEvents.OnCombatEnded += CombatEnded;

        m_inputActions = new InputSystem_Actions();
        m_inputActions.Enable();
        m_active = true;        
        
        return true;    
    }
    public bool Dispose()
    {
        m_active = false;
        CombatEvents.OnCombatStarted -= CombatStarted;
        CombatEvents.OnCombatEnded -= CombatEnded;

        m_inputActions.Disable();
        m_inputActions.Dispose();        
        return true;
    }   
}
