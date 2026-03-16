using UnityEngine;
using UnityEngine.AI;

public class AIInputSource : IInputSource
{
    private Transform m_actor;
    private Transform m_target;

    private NavMeshPath m_path = new NavMeshPath();
    public AIInputSource(Transform actor) 
    {
        m_actor = actor;
    }
    public void SetTarget(Transform actor) 
    {
        m_target = actor;
    }
    public InputState GetInput()
    {
        InputState state = new InputState();

        if (m_target == null) 
        {
            return state;
        }
        NavMesh.CalculatePath(
            m_actor.position,
            m_target.position,
            NavMesh.AllAreas,
            m_path);

        if (m_path.corners.Length < 2)
            return state;

        Vector3 nextCorner = m_path.corners[1];

        Vector3 dir = (nextCorner - m_actor.position);
        dir.y = 0;

        state.InputDirection = dir.normalized;

        return state;
    }
}
