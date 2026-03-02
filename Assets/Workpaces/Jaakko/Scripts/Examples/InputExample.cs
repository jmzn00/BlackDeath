using UnityEngine;
using UnityEngine.InputSystem;

public class InputExample : MonoBehaviour
{
    private InputManager m_inputManager;
    private InputSystem_Actions m_inputActions;
    
    private void Start()
    {
        m_inputManager = GameManager.I.Input;

        m_inputManager.InputActions.Player.Jump.performed += ctx
            =>  Debug.LogWarning("Jump Pressed");

        m_inputActions = m_inputManager.InputActions;
        m_inputActions.Player.Jump.performed += ctx =>
        {
            Debug.LogError("Jump Pressed");
        };
    }
    private void Update()
    {
        var state = m_inputManager.GetInputState();

        Debug.Log(state.InputDirection);
    }
}
