using UnityEngine;
using System;

[Serializable]
public class CameraPresetData
{
    public string presetName;
    public Vector3 positionOffset;
    public Vector3 rotation;
    public float fieldOfView = 60f;
    public float orthographicSize = 5f;
    public float followDamping = 1f;
    public float transitionSpeed = 2f;

    [Header("Follow Zoom Settings")]
    public float width = 10f;
    public float zoomDamping = 0.5f;
    public Vector2 fovRange = new Vector2(40f, 60f);

    public CameraPresetData Clone()
    {
        return new CameraPresetData
        {
            presetName = presetName,
            positionOffset = positionOffset,
            rotation = rotation,
            fieldOfView = fieldOfView,
            orthographicSize = orthographicSize,
            followDamping = followDamping,
            transitionSpeed = transitionSpeed,
            width = width,
            zoomDamping = zoomDamping,
            fovRange = fovRange
        };
    }
}
