using UnityEngine;

public class GravityModule : IEnvironmentModule
{
    private readonly MovementController m_controller;

    public GravityModule(MovementController controller)
    {
        m_controller = controller;
    }

    public void UpdateEnviroment() 
    {
        Vector3 velocity = m_controller.Velocity;

        if (!m_controller.MovmentState.IsGrounded) 
        {
            PlayerStats stats = m_controller.RuntimeStats;            

            velocity.y += stats.Gravity * Time.deltaTime;
            m_controller.Velocity = velocity;
        }
    }
}