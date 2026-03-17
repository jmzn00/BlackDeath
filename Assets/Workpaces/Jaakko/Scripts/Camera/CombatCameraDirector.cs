using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

// Attach to a scene GameObject. Configure presets in inspector.
// Cinemachine virtual camera should Follow / LookAt this director's anchor Transform.
[ExecuteAlways]
public class CombatCameraDirector : MonoBehaviour
{
    [Header("Anchor used by Cinemachine Virtual Camera (set vcam Follow/LookAt to this)")]
    [SerializeField] private Transform m_anchor;

    [Header("Presets")]
    [SerializeField] private List<CombatCameraPreset> m_presets = new();

    [Header("Editor Live Preview")]
    [Tooltip("When enabled, the director repaints the Scene view so you can preview animated CameraTarget movement in real time.")]
    public bool livePreviewInEditor = true;

    // internal smoothing state
    private Coroutine m_moveRoutine;
    private Vector3 m_velocity;
    private Quaternion m_rotationVelocity;

    // active tracking state (for continuous follow)
    private CombatCameraPreset m_activePreset;
    private Transform m_trackedTarget;
    private Transform m_trackedLookTarget;
    private CameraTargetParams m_trackedTargetParams;

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
        // cache a vcam if present in scene
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
        m_trackedTargetParams = null;

        switch (preset.anchor)
        {
            case CombatCameraPreset.AnchorType.Actor:
                // Prefer a child "camera target" (CameraTargetParams) if present, otherwise use actor root.
                m_trackedTarget = ResolveCameraTarget(actor);
                m_trackedLookTarget = m_trackedTarget;
                break;
            case CombatCameraPreset.AnchorType.Target:
                m_trackedTarget = ResolveCameraTarget(target);
                m_trackedLookTarget = m_trackedTarget;
                break;
            default:
                m_trackedTarget = null;
                m_trackedLookTarget = null;
                break;
        }

        if (m_trackedTarget != null)
            m_trackedTargetParams = m_trackedTarget.GetComponent<CameraTargetParams>();

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

        // Apply FollowZoom / FollowOffset settings now:
        // preference order:
        // 1) animated target params (if present and set to override)
        // 2) preset values (if preset.useFollowZoom == true)
        ApplyFollowZoomSettings(preset, m_trackedTargetParams);

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

    // Prefers a child object that contains CameraTargetParams (so animations can move that child).
    // Falls back to a child named "CameraTarget" (common convention) or the subject root transform.
    private Transform ResolveCameraTarget(Transform subject)
    {
        if (subject == null) return null;

        // 1) camera params component in children (most robust)
        var paramsComponent = subject.GetComponentInChildren<CameraTargetParams>(true);
        if (paramsComponent != null)
            return paramsComponent.transform;

        // 2) explicit child object named "CameraTarget"
        var named = subject.Find("CameraTarget");
        if (named != null)
            return named;

        // 3) fallback to subject root
        return subject;
    }

    private void ApplyFollowZoomSettings(CombatCameraPreset preset, CameraTargetParams targetParams)
    {
        // Ensure vcam cached
        if (m_vcam == null)
        {
            m_vcam = GameObject.FindFirstObjectByType<CinemachineCamera>();
            if (m_vcam != null)
            {
                m_followZoom = m_vcam.GetComponent<CinemachineFollowZoom>();
                m_follow = m_vcam.GetComponent<CinemachineFollow>();
            }
        }

        // If animated target provides overrides, prefer those.
        if (targetParams != null && targetParams.overrideFollowZoom && m_followZoom != null)
        {
            // smooth or instant depending on target param
            if (targetParams.followZoomSmoothTime <= 0f)
                m_followZoom.FovRange = targetParams.followZoomFovRange;
            else
            {
                if (m_zoomRoutine != null) { StopCoroutine(m_zoomRoutine); m_zoomRoutine = null; }
                m_zoomRoutine = StartCoroutine(ZoomRoutine(m_followZoom.FovRange, targetParams.followZoomFovRange, targetParams.followZoomSmoothTime));
            }
        }
        else if (preset.useFollowZoom && m_followZoom != null)
        {
            // use preset zoom
            if (preset.followZoomSmoothTime <= 0f)
                m_followZoom.FovRange = preset.followZoomFovRange;
            else
            {
                if (m_zoomRoutine != null) { StopCoroutine(m_zoomRoutine); m_zoomRoutine = null; }
                m_zoomRoutine = StartCoroutine(ZoomRoutine(m_followZoom.FovRange, preset.followZoomFovRange, preset.followZoomSmoothTime));
            }
        }

        // follow offset (instant if override requested)
        if (targetParams != null && targetParams.overrideFollowOffset && m_follow != null)
        {
            m_follow.FollowOffset = targetParams.followOffset;
        }
        else if (preset.overrideFollowOffset && m_follow != null)
        {
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

        // Dynamic per-frame follow-zoom / follow-offset from animated target params (if present).
        if (m_trackedTargetParams != null && m_vcam != null)
        {
            // apply dynamic zoom overrides
            if (m_trackedTargetParams.overrideFollowZoom && m_followZoom != null)
            {
                float smooth = Mathf.Max(0.0001f, m_trackedTargetParams.followZoomSmoothTime);
                Vector2 cur = m_followZoom.FovRange;
                Vector2 goal = m_trackedTargetParams.followZoomFovRange;
                m_followZoom.FovRange = Vector2.Lerp(cur, goal, Time.unscaledDeltaTime / smooth);
            }

            // apply dynamic follow offset overrides
            if (m_trackedTargetParams.overrideFollowOffset && m_follow != null)
            {
                float smooth = Mathf.Max(0.0001f, Mathf.Max(0.01f, m_activePreset.moveSmoothTime));
                Vector3 curOff = m_follow.FollowOffset;
                Vector3 goalOff = m_trackedTargetParams.followOffset;
                m_follow.FollowOffset = Vector3.Lerp(curOff, goalOff, Time.unscaledDeltaTime / smooth);
            }
        }

#if UNITY_EDITOR
        // In the editor, optionally repaint the scene view each frame so animated target moves are visible immediately.
        if (!Application.isPlaying && livePreviewInEditor)
            SceneView.RepaintAll();
#endif
    }

    private void OnDrawGizmos()
    {
        if (m_anchor == null) return;

        // anchor gizmo
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(m_anchor.position, 0.15f);
        Gizmos.DrawIcon(m_anchor.position, "d_Camera.png", true);

        // draw line to tracked target
        if (m_trackedTarget != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(m_anchor.position, m_trackedTarget.position);
            Gizmos.DrawWireSphere(m_trackedTarget.position, 0.075f);
        }

        // draw look-at direction
        if (m_trackedLookTarget != null)
        {
            Vector3 lookPos = m_trackedLookTarget.position + (m_activePreset != null ? m_activePreset.lookAtOffset : Vector3.zero);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(m_anchor.position, lookPos);
            Gizmos.DrawWireSphere(lookPos, 0.06f);
        }

        // optional: draw simple camera frustum approximation using main camera FOV (scene-only helper)
#if UNITY_EDITOR
        Camera sceneCam = Camera.main;
        if (sceneCam != null)
        {
            Gizmos.color = new Color(1f, 0.5f, 0.1f, 0.4f);
            DrawSimpleFrustumGizmo(m_anchor.position, m_anchor.forward, sceneCam.fieldOfView, sceneCam.aspect, 1.5f);
        }
#endif
    }

#if UNITY_EDITOR
    private void DrawSimpleFrustumGizmo(Vector3 origin, Vector3 forward, float fov, float aspect, float distance)
    {
        float halfHeight = Mathf.Tan(fov * Mathf.Deg2Rad * 0.5f) * distance;
        float halfWidth = halfHeight * aspect;

        Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
        Vector3 up = Vector3.Cross(forward, right).normalized;

        Vector3 center = origin + forward * distance;
        Vector3 tl = center + up * halfHeight - right * halfWidth;
        Vector3 tr = center + up * halfHeight + right * halfWidth;
        Vector3 bl = center - up * halfHeight - right * halfWidth;
        Vector3 br = center - up * halfHeight + right * halfWidth;

        Gizmos.DrawLine(origin, tl);
        Gizmos.DrawLine(origin, tr);
        Gizmos.DrawLine(origin, bl);
        Gizmos.DrawLine(origin, br);

        Gizmos.DrawLine(tl, tr);
        Gizmos.DrawLine(tr, br);
        Gizmos.DrawLine(br, bl);
        Gizmos.DrawLine(bl, tl);
    }
#endif
}