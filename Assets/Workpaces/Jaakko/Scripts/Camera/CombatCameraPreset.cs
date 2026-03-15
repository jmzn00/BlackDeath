using UnityEngine;

[CreateAssetMenu(menuName = "Camera/Combat Camera Preset", fileName = "CombatCameraPreset")]
public class CombatCameraPreset : ScriptableObject
{
    public string presetId;

    public enum AnchorType
    {
        Actor,
        Target,
        World
    }

    [Header("Anchor")]
    public AnchorType anchor = AnchorType.Actor;

    // if Anchor == World, use this world position as base
    public Vector3 worldPosition;

    // offset applied to anchor world position
    public Vector3 offset = Vector3.zero;

    // look offset (optional)
    public Vector3 lookAtOffset = Vector3.zero;

    [Header("Movement")]
    // whether to smooth (true) or snap immediately (false)
    public bool smooth = true;

    // movement smoothing time (seconds) when smooth == true
    public float moveSmoothTime = 0.5f;

    // rotation smoothing time (seconds)
    public float rotationSmoothTime = 0.3f;

    // how long the blend should feel (informational, director uses smoothing values)
    public float blendDuration => smooth ? Mathf.Max(moveSmoothTime, rotationSmoothTime) : 0f;

    [Header("Follow Zoom (Cinemachine FollowZoom)")]
    // enable applying FollowZoom settings when this preset is activated
    public bool useFollowZoom = false;

    // target FOV range for CinemachineFollowZoom (min,max)
    public Vector2 followZoomFovRange = new Vector2(40f, 60f);

    // follow offset for CinemachineFollow component (x,y,z)
    public Vector3 followOffset = new Vector3(0f, 5f, -10f);

    // whether to apply follow offset to the VCAM's CinemachineFollow component
    public bool overrideFollowOffset = false;

    // smoothing time for zoom/offset transitions (seconds). 0 = instant
    [Min(0f)]
    public float followZoomSmoothTime = 0.25f;
}