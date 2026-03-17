using UnityEngine;

/// <summary>
/// Attach to an actor's `CameraTarget` child. Fields are animatable by the actor's animation clips.
/// The director will read these at runtime (when tracking this target) and apply to the Cinemachine FollowZoom / Follow.
/// </summary>
public class CameraTargetParams : MonoBehaviour
{
    [Header("Follow Zoom override")]
    [Tooltip("If true the director will use these values (animated) instead of preset FollowZoom values.")]
    public bool overrideFollowZoom = false;

    [Tooltip("Target FOV range used by CinemachineFollowZoom.")]
    public Vector2 followZoomFovRange = new Vector2(40f, 60f);

    [Tooltip("If true, override CinemachineFollow.FollowOffset.")]
    public bool overrideFollowOffset = false;

    [Tooltip("Follow offset applied to CinemachineFollow (animated).")]
    public Vector3 followOffset = new Vector3(0f, 5f, -10f);

    [Tooltip("Smoothing time used when transitioning follow zoom / offsets. 0 = instant.")]
    public float followZoomSmoothTime = 0.25f;
}