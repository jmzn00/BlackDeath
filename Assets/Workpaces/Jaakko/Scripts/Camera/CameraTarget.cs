using UnityEngine;

/// <summary>
/// Placed as child of CombatActor. Provides target for camera to follow.
/// Can be animated with keyframes during combat animations.
/// </summary>
public class CameraTarget : ActorComponentBase
{
    [Header("Runtime Zoom Control")]
    [Tooltip("Zoom level for this target. Can be animated via keyframes.")]
    [Range(0.1f, 3f)]
    public float zoomMultiplier = 1f;

    [Header("Offset")]
    public Vector3 localOffset = Vector3.zero;

    public CombatActor Actor { get; private set; }
    public Vector3 WorldPosition => transform.position + localOffset;

    /// <summary>
    /// Called by animation events to notify camera system of important moments
    /// </summary>
    public override void OnActorComponentsInitialized(Actor actor)
    {
        base.OnActorComponentsInitialized(actor);

        Actor = actor.Get<CombatActor>();
    }
    public void TriggerCameraEvent(string eventName)
    {
        CameraAnimationEvents.NotifyCameraEvent(this, eventName);
    }
}
