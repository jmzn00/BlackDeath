using UnityEngine;

public class TestScript : MonoBehaviour
{
    private InputManager m_inputManager;
    private void Start()
    {
        m_inputManager = GameManager.I.Input;
        m_inputManager.InputActions.Player.Jump.performed += ctx
            =>  Debug.LogWarning("Jump Pressed");
    }
    private void Update()
    {
        var state = m_inputManager.GetInputState();

        Debug.Log(state.InputDirection);
    }
}
