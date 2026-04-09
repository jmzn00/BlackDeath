using UnityEngine;

[ExecuteAlways]
public class TransformFollow : MonoBehaviour
{
    [SerializeField] private Transform m_target;

    private void Update()
    {
        if (m_target == null) return;

        transform.position = m_target.position;
        transform.rotation = m_target.rotation;
    }
}
