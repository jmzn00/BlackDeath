using UnityEngine;

public class PlayerInputSource : IInputSource
{
    private InputManager m_inputManager;

    public PlayerInputSource(InputManager inputManager)
    {
        m_inputManager = inputManager;
    }
    public InputState GetInput()
    {
        return m_inputManager.GetInputState();
    }
}
