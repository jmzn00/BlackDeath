using UnityEngine;

public class GroundFrictionModule : IEnvironmentModule
{
    private MovementController m_controller;
    public GroundFrictionModule(MovementController controller) 
    {
        m_controller = controller;
    }
    public void UpdateEnviroment() 
    {
        if (!m_controller.MovmentState.IsGrounded
            || m_controller.MovmentState.IsSliding) return;

        PlayerStats stats = m_controller.RuntimeStats;
        Vector3 velocity = m_controller.Velocity;

        // Horizontal velocity only
        Vector3 horizontalVel = new Vector3(velocity.x, 0, velocity.z);

        // Forward/back friction
        horizontalVel = HandleAxisFriction(horizontalVel, m_controller.Player.forward, m_controller.InputState.InputDirection.y,
            stats.Acceleration, stats.TimeToStop, stats.SlideFactor, stats.BrakeFactor);

        // Strafe friction
        horizontalVel = HandleAxisFriction(horizontalVel, m_controller.Player.right, m_controller.InputState.InputDirection.x,
            stats.Acceleration, stats.TimeToStop, stats.SlideFactor, stats.BrakeFactor);

        // Apply horizontal velocity while keeping vertical velocity intact
        m_controller.Velocity = new Vector3(horizontalVel.x, velocity.y, horizontalVel.z);
    }
    private Vector3 HandleAxisFriction(Vector3 velocity, Vector3 axis, float input, float accel, float stopTime, float slideFactor, float brakeFactor)
    {
        axis.Normalize();
        float currentSpeed = Vector3.Dot(velocity, axis);

        if (Mathf.Abs(input) < 0.01f)
        {
            // No input  slide naturally over stopTime
            float decel = Mathf.Abs(currentSpeed) / stopTime * Time.deltaTime;
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0, decel);
        }
        else
        {
            // Opposite input  brake faster
            if (Mathf.Sign(currentSpeed) != Mathf.Sign(input) && Mathf.Abs(currentSpeed) > 0.01f)
            {
                float wishSpeed = input * Mathf.Abs(currentSpeed);
                float decel = Mathf.Abs(currentSpeed - wishSpeed) * brakeFactor * Time.deltaTime;
                currentSpeed = Mathf.MoveTowards(currentSpeed, wishSpeed, decel);
            }
        }

        // Update velocity along this axis
        velocity -= axis * Vector3.Dot(velocity, axis);
        velocity += axis * currentSpeed;
        return velocity;
    }
}
