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

        Vector3 targetPos = m_target.position - m_target.right * 2f 
            - m_target.forward * 2f;

        NavMesh.CalculatePath(
            m_actor.position,
            targetPos,
            NavMesh.AllAreas,
            m_path);

        if (m_path.corners.Length < 2)
            return state;

        Vector3 nextCorner = m_path.corners[1];

        Vector3 dir = (nextCorner - m_actor.position);
        dir.y = 0;

        Vector3 localDir = m_actor.InverseTransformDirection(dir);

        state.InputDirection = new Vector2(localDir.x, localDir.z);

        float distSq = (m_target.position - m_actor.position).sqrMagnitude;
        state.RunHeld = distSq > 9f;

        return state;
    }
}
