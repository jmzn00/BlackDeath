using UnityEngine;

/// <summary>
/// Placed as child of CombatActor. Provides target for camera to follow.
/// Can be animated with keyframes during combat animations.
/// </summary>
public class CameraTarget : MonoBehaviour
{
    [Header("Runtime Zoom Control")]
    [Tooltip("Zoom level for this target. Can be animated via keyframes.")]
    [Range(0.1f, 3f)]
    public float zoomMultiplier = 1f;

    [Header("Offset")]
    public Vector3 localOffset = Vector3.zero;

    private CombatActor m_actor;

    private void Awake()
    {
        m_actor = GetComponentInParent<CombatActor>();
    }

    public CombatActor Actor => m_actor;
    public Vector3 WorldPosition => transform.position + localOffset;

    /// <summary>
    /// Called by animation events to notify camera system of important moments
    /// </summary>
    public void TriggerCameraEvent(string eventName)
    {
        CameraAnimationEvents.NotifyCameraEvent(this, eventName);
    }
}
