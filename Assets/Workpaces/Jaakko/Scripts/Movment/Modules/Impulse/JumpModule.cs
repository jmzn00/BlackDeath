using UnityEngine;

public class JumpModule : IImpulseModule
{
    private readonly MovementController m_controller;
    private float m_coyoteTimer = 0f;

    public JumpModule(MovementController controller)
    {
        m_controller = controller;
    }

    public void UpdateImpulse()
    {
        PlayerStats stats = m_controller.RuntimeStats;

        if (m_controller.InputState.JumpPressed && m_controller.MovmentState.IsGrounded)
        {
            Vector3 velocity = m_controller.Velocity;
            velocity.y = stats.JumpForce;
            m_controller.Velocity = velocity;
        }
    }
    bool CanJump() 
    {
        return true;
    }
}