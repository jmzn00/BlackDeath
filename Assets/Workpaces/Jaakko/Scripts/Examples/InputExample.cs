using UnityEngine;

public class InputExample : Actor
{
    private InputManager m_inputManager;
    private InputSystem_Actions m_inputActions;

    public override void Init(GameManager game)
    {
        m_inputManager = game.Resolve<InputManager>();

        m_inputManager.InputActions.Player.Jump.performed += ctx
    => Debug.LogWarning("Jump Pressed");

        m_inputActions = m_inputManager.InputActions;
        m_inputActions.Player.Jump.performed += ctx =>
        {
            Debug.LogError("Jump Pressed");
        };
    }
    public override void Dispose()
    {
        base.Dispose();
    }      
    private void Update()
    {
        if (m_inputManager == null) return;

        // recommended usage is to cache the manager references needed
        var state = m_inputManager.GetInputState();

        // Alternitively, managers can be resolved directly from the Services locator,
        // but its generally better to get references in Init() and cache them for later use

        //var state = Services.Get<InputManager>().GetInputState();

        Debug.Log(state.InputDirection);
    }
}
