using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

// Attach to a scene GameObject. Configure presets in inspector.
// Cinemachine virtual camera should Follow / LookAt this director's anchor Transform.
public class CombatCameraDirector : MonoBehaviour
{
    [Header("Anchor used by Cinemachine Virtual Camera (set vcam Follow/LookAt to this)")]
    [SerializeField] private Transform m_anchor;

    [Header("Presets")]
    [SerializeField] private List<CombatCameraPreset> m_presets = new();

    // internal smoothing state
    private Coroutine m_moveRoutine;
    private Vector3 m_velocity;
    private Quaternion m_rotationVelocity;

    // active tracking state (for continuous follow)
    private CombatCameraPreset m_activePreset;
    private Transform m_trackedTarget;
    private Transform m_trackedLookTarget;

    // Cinemachine components (optional)
    private CinemachineCamera m_vcam;
    private CinemachineFollowZoom m_followZoom;
    private CinemachineFollow m_follow;
    private Coroutine m_zoomRoutine;

    private void Reset()
    {
        if (m_anchor == null)
        {
            var go = new GameObject("CameraAnchor");
            go.transform.SetParent(transform, false);
            m_anchor = go.transform;
        }
    }

    private void Awake()
    {
        // try to find the Cinemachine vcam in scene and cache zoom/follow components if present
        if (m_vcam == null)
        {
            m_vcam = GameObject.FindFirstObjectByType<CinemachineCamera>();
            if (m_vcam != null)
            {
                m_followZoom = m_vcam.GetComponent<CinemachineFollowZoom>();
                m_follow = m_vcam.GetComponent<CinemachineFollow>();
            }
        }
    }

    public CombatCameraPreset GetPreset(string id)
    {
        return m_presets.Find(p => p != null && p.presetId == id);
    }

    // actor / target may be null depending on the anchor type in the preset
    public void ApplyPreset(CombatCameraPreset preset, Transform actor, Transform target)
    {
        if (preset == null || m_anchor == null)
            return;

        // set active preset so Update can keep tracking if needed
        m_activePreset = preset;

        // determine which transform to track continuously (if any)
        m_trackedTarget = null;
        m_trackedLookTarget = null;
        switch (preset.anchor)
        {
            case CombatCameraPreset.AnchorType.Actor:
                m_trackedTarget = actor;
                m_trackedLookTarget = actor;
                break;
            case CombatCameraPreset.AnchorType.Target:
                m_trackedTarget = target;
                m_trackedLookTarget = target;
                break;
            default:
                m_trackedTarget = null;
                m_trackedLookTarget = null;
                break;
        }

        Vector3 basePos = preset.anchor == CombatCameraPreset.AnchorType.World
            ? preset.worldPosition
            : (m_trackedTarget != null ? m_trackedTarget.position : Vector3.zero);

        Vector3 desiredPos = basePos + preset.offset;
        Vector3 desiredLookPos = basePos + preset.lookAtOffset;

        // stop any one-off motion coroutine
        if (m_moveRoutine != null)
        {
            StopCoroutine(m_moveRoutine);
            m_moveRoutine = null;
        }

        // Apply FollowZoom settings (if available)
        ApplyFollowZoomSettings(preset);

        // If we are tracking a transform continuously, do an initial smooth move (if requested)
        // and then keep tracking in Update(). If not tracking, behave like before.
        if (m_trackedTarget != null)
        {
            if (!preset.smooth)
            {
                // snap into tracking immediately
                m_anchor.position = desiredPos;
                m_anchor.rotation = Quaternion.LookRotation((desiredLookPos - m_anchor.position).normalized, Vector3.up);
            }
            else
            {
                // perform a single smooth move to the initial desired position, then Update() will continue smoothing each frame
                m_moveRoutine = StartCoroutine(MoveToAndKeepTracking(desiredPos, desiredLookPos, preset.moveSmoothTime, preset.rotationSmoothTime));
            }
            return;
        }

        // no continuous track target; either snap or perform a one-off smooth move
        if (!preset.smooth)
        {
            m_anchor.position = desiredPos;
            m_anchor.rotation = Quaternion.LookRotation((desiredLookPos - m_anchor.position).normalized, Vector3.up);
            return;
        }

        m_moveRoutine = StartCoroutine(MoveAnchorRoutine(desiredPos, desiredLookPos, preset.moveSmoothTime, preset.rotationSmoothTime));
    }

    private void ApplyFollowZoomSettings(CombatCameraPreset preset)
    {
        if (!preset.useFollowZoom)
            return;

        // ensure vcam and components are resolved
        if (m_vcam == null)
        {
            m_vcam = GameObject.FindFirstObjectByType<CinemachineCamera>();
            if (m_vcam != null)
            {
                m_followZoom = m_vcam.GetComponent<CinemachineFollowZoom>();
                m_follow = m_vcam.GetComponent<CinemachineFollow>();
            }
        }

        if (m_followZoom != null)
        {
            // stop existing zoom routine
            if (m_zoomRoutine != null)
            {
                StopCoroutine(m_zoomRoutine);
                m_zoomRoutine = null;
            }

            if (preset.followZoomSmoothTime <= 0f)
            {
                m_followZoom.FovRange = preset.followZoomFovRange;
            }
            else
            {
                m_zoomRoutine = StartCoroutine(ZoomRoutine(m_followZoom.FovRange, preset.followZoomFovRange, preset.followZoomSmoothTime));
            }
        }

        if (preset.overrideFollowOffset && m_follow != null)
        {
            // apply follow offset instantly for now; could be smoothed similarly if desired
            m_follow.FollowOffset = preset.followOffset;
        }
    }

    private IEnumerator ZoomRoutine(Vector2 from, Vector2 to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            if (m_followZoom != null)
                m_followZoom.FovRange = Vector2.Lerp(from, to, t);
            yield return null;
        }
        if (m_followZoom != null)
            m_followZoom.FovRange = to;
        m_zoomRoutine = null;
    }

    private IEnumerator MoveToAndKeepTracking(Vector3 targetPos, Vector3 lookAtPos, float moveSmooth, float rotSmooth)
    {
        // same behavior as MoveAnchorRoutine but after finishing keep m_trackedTarget non-null so Update() continues to follow
        float elapsed = 0f;
        while (elapsed < Mathf.Max(moveSmooth, rotSmooth))
        {
            elapsed += Time.unscaledDeltaTime;

            if (moveSmooth > 0f)
                m_anchor.position = Vector3.SmoothDamp(m_anchor.position, targetPos, ref m_velocity, moveSmooth, Mathf.Infinity, Time.unscaledDeltaTime);
            else
                m_anchor.position = targetPos;

            Quaternion desiredRot = Quaternion.LookRotation((lookAtPos - m_anchor.position).normalized, Vector3.up);
            if (rotSmooth > 0f)
                m_anchor.rotation = Quaternion.Slerp(m_anchor.rotation, desiredRot, Time.unscaledDeltaTime / rotSmooth);
            else
                m_anchor.rotation = desiredRot;

            yield return null;
        }

        m_anchor.position = targetPos;
        m_anchor.rotation = Quaternion.LookRotation((lookAtPos - m_anchor.position).normalized, Vector3.up);
        m_moveRoutine = null;
    }

    private IEnumerator MoveAnchorRoutine(Vector3 targetPos, Vector3 lookAtPos, float moveSmooth, float rotSmooth)
    {
        float elapsed = 0f;
        // we will smooth using a damp approach (unscaled time so it feels consistent)
        while (elapsed < Mathf.Max(moveSmooth, rotSmooth))
        {
            elapsed += Time.unscaledDeltaTime;

            // position: SmoothDamp towards target
            if (moveSmooth > 0f)
                m_anchor.position = Vector3.SmoothDamp(m_anchor.position, targetPos, ref m_velocity, moveSmooth, Mathf.Infinity, Time.unscaledDeltaTime);
            else
                m_anchor.position = targetPos;

            // rotation: slerp to look at
            Quaternion desiredRot = Quaternion.LookRotation((lookAtPos - m_anchor.position).normalized, Vector3.up);
            if (rotSmooth > 0f)
                m_anchor.rotation = Quaternion.Slerp(m_anchor.rotation, desiredRot, Time.unscaledDeltaTime / rotSmooth);
            else
                m_anchor.rotation = desiredRot;

            yield return null;
        }

        // ensure final values
        m_anchor.position = targetPos;
        m_anchor.rotation = Quaternion.LookRotation((lookAtPos - m_anchor.position).normalized, Vector3.up);
        m_moveRoutine = null;
    }

    private void Update()
    {
        // Continuous tracking: when m_trackedTarget is set (preset anchor Actor/Target), keep the anchor following that transform.
        if (m_trackedTarget == null || m_activePreset == null)
            return;

        // If a one-off coroutine is running we let it handle the initial approach; still update rotation/look each frame to be consistent.
        float moveSmooth = m_activePreset.moveSmoothTime;
        float rotSmooth = m_activePreset.rotationSmoothTime;

        Vector3 basePos = m_trackedTarget != null ? m_trackedTarget.position : m_activePreset.worldPosition;
        Vector3 desiredPos = basePos + m_activePreset.offset;
        Vector3 desiredLookPos = (m_trackedLookTarget != null ? m_trackedLookTarget.position : basePos) + m_activePreset.lookAtOffset;

        if (m_activePreset.smooth)
        {
            if (m_moveRoutine == null) // if a coroutine is already running, don't double-smooth position here
                m_anchor.position = Vector3.SmoothDamp(m_anchor.position, desiredPos, ref m_velocity, moveSmooth, Mathf.Infinity, Time.unscaledDeltaTime);
            // rotation smoothing each frame
            Quaternion desiredRot = Quaternion.LookRotation((desiredLookPos - m_anchor.position).normalized, Vector3.up);
            if (rotSmooth > 0f)
                m_anchor.rotation = Quaternion.Slerp(m_anchor.rotation, desiredRot, Time.unscaledDeltaTime / rotSmooth);
            else
                m_anchor.rotation = desiredRot;
        }
        else
        {
            // snap
            m_anchor.position = desiredPos;
            m_anchor.rotation = Quaternion.LookRotation((desiredLookPos - m_anchor.position).normalized, Vector3.up);
        }
    }
}