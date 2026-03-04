using UnityEngine;

public class WalkRunModule : IIntentModule
{
    private MovementController m_controller;
    private float m_movementHeldTime;
    public WalkRunModule(MovementController controller) 
    {
        m_controller = controller;
    }
    public void UpdateIntent() 
    {
        if (!m_controller.MovmentState.IsGrounded
            || m_controller.MovmentState.IsSliding) return;

        PlayerStats stats = m_controller.RuntimeStats;
        Transform player = m_controller.Player;

        Vector2 input = m_controller.InputState.InputDirection;
        if (input.sqrMagnitude > 1f) input.Normalize();

        float targetSpeed = (m_movementHeldTime >= stats.TimeToRun)
            ? stats.RunSpeed : stats.WalkSpeed;


        Vector3 vel = m_controller.Velocity;
        Vector3 groundNormal = m_controller.MovmentState.ContactNormal;

        Vector3 forward = Vector3.ProjectOnPlane(player.forward, groundNormal).normalized;
        Vector3 right = Vector3.ProjectOnPlane(player.right, groundNormal).normalized;

        Vector3 hor = Vector3.ProjectOnPlane(vel, groundNormal);

        hor = AccelerateAlong(hor, forward, input.y, targetSpeed, stats.Acceleration);
        hor = AccelerateAlong(hor, right, input.x, targetSpeed, stats.Acceleration);

        m_controller.Velocity = hor + Vector3.Project(vel, groundNormal);
    }
    private Vector3 AccelerateAlong(Vector3 velocity, Vector3 axis, float input, float maxSpeed, float accel)
    { 
        axis.Normalize();

        if (Mathf.Abs(input) < 0.01f)
            return velocity;

        float currentSpeed = Vector3.Dot(velocity, axis);
        float wishSpeed = input * maxSpeed;
        float addSpeed = accel * Time.deltaTime;
        float newSpeed = Mathf.MoveTowards(currentSpeed, wishSpeed, addSpeed);
        velocity -= axis * currentSpeed; velocity += axis * newSpeed;
        return velocity;
    }

}
