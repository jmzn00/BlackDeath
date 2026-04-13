using System;
using UnityEngine;

/// <summary>
/// Static event system for camera-related animation events
/// </summary>
public static class CameraAnimationEvents
{
    public static event Action<CameraTarget, string> OnCameraEventTriggered;
    public static event Action<CameraTarget> OnTargetChanged;
    public static event Action<float> OnZoomChanged;

    public static void NotifyCameraEvent(CameraTarget target, string eventName)
    {
        OnCameraEventTriggered?.Invoke(target, eventName);
    }

    public static void NotifyTargetChanged(CameraTarget target)
    {
        OnTargetChanged?.Invoke(target);
    }

    public static void NotifyZoomChanged(float zoom)
    {
        OnZoomChanged?.Invoke(zoom);
    }
}
