using UnityEngine;
using Unity.Cinemachine;
using System.Collections.Generic;
using System.Linq;

public class CombatCameraMode : ICameraMode
{
    private CombatManager m_combatManager;
    private CameraManager m_cameraManager;
    private GameManager m_gameManager;
    private CinemachineCamera m_camera;
    private CameraPresetsConfig m_presets;

    private CameraTarget m_currentTarget;
    private CameraPresetData m_currentPreset;
    private CameraPresetType m_currentPresetType;

    private bool m_isAnimationControlled;
    private Dictionary<CombatActor, CameraTarget> m_actorTargets;

    // State tracking
    private CombatActor m_currentActor;
    private CombatState m_currentCombatState;
    private CombatActorState m_currentActorState;

    // Pause state tracking
    private bool m_wasPausedLastFrame = false;
    
    // Smooth transition state
    private bool m_isTransitioning = false;
    private float m_transitionProgress = 0f;
    private CameraPresetData m_previousPreset;
    private Vector3 m_previousOffset;
    private float m_previousDamping;
    private float m_previousWidth;

    public CombatCameraMode(CombatManager combatManager, CameraManager cameraManager, GameManager gameManager)
    {
        m_combatManager = combatManager;
        m_cameraManager = cameraManager;
        m_gameManager = gameManager;
        m_camera = cameraManager.Camera;
        m_actorTargets = new Dictionary<CombatActor, CameraTarget>();

        Debug.Log("CombatCameraMode: Constructor called");

        // Load presets from Resources
        m_presets = Resources.Load<CameraPresetsConfig>("CameraPresets");
        if (m_presets == null)
        {
            Debug.LogError("CombatCameraMode: CameraPresetsConfig not found in Resources folder!");
        }
        else
        {
            Debug.Log("CombatCameraMode: Loaded CameraPresetsConfig successfully");
        }
    }

    public bool CanEnter()
    {
        bool canEnter = m_gameManager.State == GameState.Combat;
        return canEnter;
    }

    public void Enter()
    {
        Debug.Log("========================================");
        Debug.Log("=== Combat Camera Mode: ENTER ===");
        Debug.Log("========================================");

        // Subscribe to combat events
        CombatEvents.OnTurnStarted += OnTurnStarted;
        CombatEvents.OnTurnEnded += OnTurnEnded;
        CombatEvents.OnCombatStateChanged += OnCombatStateChanged;
        CombatEvents.OnActorStateChanged += OnActorStateChanged;
        CombatEvents.OnActionSubmitted += OnActionSubmitted;
        CombatEvents.OnActionResolved += OnActionResolved;
        CombatEvents.OnCombatActorsChanged += OnCombatActorsChanged;
        CombatEvents.OnCombatEnded += OnCombatEnded;
        CombatEvents.OnTransitionStarted += OnTransitionStarted;
        CombatEvents.OnTransitionEnded += OnTransitionEnded;

        // Subscribe to animation events
        CameraAnimationEvents.OnTargetChanged += OnAnimationTargetChanged;
        CameraAnimationEvents.OnZoomChanged += OnAnimationZoomChanged;

        Debug.Log("CombatCameraMode: All event subscriptions complete");

        // Initialize actor targets
        RefreshActorTargets();

        // Set initial preset
        ApplyPreset(CameraPresetType.TurnStart);
        
        Debug.Log("CombatCameraMode: Enter complete");
    }

    public void Exit()
    {
        Debug.Log("=== Combat Camera Mode: EXIT ===");

        // Unsubscribe from events
        CombatEvents.OnTurnStarted -= OnTurnStarted;
        CombatEvents.OnTurnEnded -= OnTurnEnded;
        CombatEvents.OnCombatStateChanged -= OnCombatStateChanged;
        CombatEvents.OnActorStateChanged -= OnActorStateChanged;
        CombatEvents.OnActionSubmitted -= OnActionSubmitted;
        CombatEvents.OnActionResolved -= OnActionResolved;
        CombatEvents.OnCombatActorsChanged -= OnCombatActorsChanged;
        CombatEvents.OnCombatEnded -= OnCombatEnded;
        CombatEvents.OnTransitionStarted -= OnTransitionStarted;
        CombatEvents.OnTransitionEnded -= OnTransitionEnded;

        CameraAnimationEvents.OnTargetChanged -= OnAnimationTargetChanged;
        CameraAnimationEvents.OnZoomChanged -= OnAnimationZoomChanged;

        m_currentTarget = null;
        m_currentActor = null;
        m_actorTargets.Clear();
    }

    public void Update(float dt)
    {
#if UNITY_EDITOR
        // In editor, detect pause state changes
        bool isPaused = UnityEditor.EditorApplication.isPaused;
        
        if (isPaused != m_wasPausedLastFrame)
        {
            if (isPaused)
            {
                Debug.Log("CombatCameraMode: Game paused - Camera manipulation enabled");
            }
            else
            {
                Debug.Log("CombatCameraMode: Game resumed - Reapplying current preset");
                if (m_currentPreset != null)
                {
                    UpdateCinemachineFollow();
                    ApplyZoomFromTarget();
                }
            }
            m_wasPausedLastFrame = isPaused;
        }

        // Skip updates when paused to allow manual manipulation
        if (isPaused)
        {
            return;
        }
#endif

        if (m_currentTarget == null || m_currentPreset == null) return;

        // Handle smooth transitions between presets
        if (m_isTransitioning)
        {
            UpdateTransition(dt);
        }
        else if (m_isAnimationControlled)
        {
            UpdateAnimationControlled(dt);
        }
        else
        {
            UpdatePresetControlled(dt);
        }
    }

    private void UpdateTransition(float dt)
    {
        if (m_previousPreset == null || m_currentPreset == null)
        {
            m_isTransitioning = false;
            return;
        }

        m_transitionProgress += dt * m_currentPreset.transitionSpeed;

        if (m_transitionProgress >= 1f)
        {
            m_transitionProgress = 1f;
            m_isTransitioning = false;
            Debug.Log("Camera preset transition complete");
        }

        float t = EaseInOutCubic(m_transitionProgress);

        Vector3 targetOffset = Vector3.Lerp(m_previousOffset, m_currentPreset.positionOffset, t);
        float targetDamping = Mathf.Lerp(m_previousDamping, m_currentPreset.followDamping, t);
        float targetWidth = Mathf.Lerp(m_previousWidth, m_currentPreset.width, t);

        var follow = m_camera.GetComponent<CinemachineFollow>();
        if (follow != null)
        {
            m_camera.Follow = m_currentTarget.transform;
            follow.FollowOffset = targetOffset;
            follow.TrackerSettings.PositionDamping = new Vector3(targetDamping, targetDamping, targetDamping);
        }

        var followZoom = m_camera.GetComponent<CinemachineFollowZoom>();
        if (followZoom != null)
        {
            followZoom.Width = targetWidth * m_currentTarget.zoomMultiplier;
            followZoom.Damping = m_currentPreset.zoomDamping;
            followZoom.FovRange = m_currentPreset.fovRange;
        }
    }

    private void UpdatePresetControlled(float dt)
    {
        UpdateCinemachineFollow();
        ApplyZoomFromTarget();
    }

    private void UpdateAnimationControlled(float dt)
    {
        UpdateCinemachineFollow();
        ApplyZoomFromTarget();
    }

    private void UpdateCinemachineFollow()
    {
        if (m_currentTarget == null || m_currentPreset == null) return;

        var follow = m_camera.GetComponent<CinemachineFollow>();
        if (follow != null)
        {
            m_camera.Follow = m_currentTarget.transform;
            follow.FollowOffset = m_currentPreset.positionOffset;
            follow.TrackerSettings.PositionDamping = new Vector3(
                m_currentPreset.followDamping,
                m_currentPreset.followDamping,
                m_currentPreset.followDamping
            );
        }
    }

    private void ApplyZoomFromTarget()
    {
        if (m_currentTarget == null || m_currentPreset == null) return;

        var followZoom = m_camera.GetComponent<CinemachineFollowZoom>();
        if (followZoom != null)
        {
            followZoom.Width = m_currentPreset.width * m_currentTarget.zoomMultiplier;
            followZoom.Damping = m_currentPreset.zoomDamping;
            followZoom.FovRange = m_currentPreset.fovRange;
        }
    }

    private void ApplyPreset(CameraPresetType presetType)
    {
        if (m_presets == null)
        {
            Debug.LogError("CombatCameraMode: Cannot apply preset - m_presets is null");
            return;
        }

        // Store previous preset values for smooth transition
        if (m_currentPreset != null)
        {
            m_previousPreset = m_currentPreset;
            
            var follow = m_camera.GetComponent<CinemachineFollow>();
            if (follow != null)
            {
                m_previousOffset = follow.FollowOffset;
                m_previousDamping = follow.TrackerSettings.PositionDamping.x;
            }
            
            var followZoom = m_camera.GetComponent<CinemachineFollowZoom>();
            if (followZoom != null)
            {
                m_previousWidth = followZoom.Width;
            }
        }

        m_currentPresetType = presetType;
        m_currentPreset = m_presets.GetPreset(presetType);
        m_isAnimationControlled = false;

        if (m_currentPreset == null)
        {
            Debug.LogError($"CombatCameraMode: Preset {presetType} is null!");
            return;
        }

        // Start transition if we had a previous preset
        if (m_previousPreset != null && m_currentPreset.transitionSpeed > 0)
        {
            m_isTransitioning = true;
            m_transitionProgress = 0f;
            Debug.Log($"Starting smooth transition to preset: {presetType} (speed: {m_currentPreset.transitionSpeed})");
        }
        else
        {
            m_isTransitioning = false;
            UpdateCinemachineFollow();
            ApplyZoomFromTarget();
            Debug.Log($"Applied camera preset immediately: {presetType}");
        }
    }

    private float EaseInOutCubic(float t)
    {
        return t < 0.5f 
            ? 4f * t * t * t 
            : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
    }

    private void SetTarget(CombatActor actor)
    {
        if (actor == null)
        {
            Debug.LogWarning("CombatCameraMode: Attempted to set null actor as target");
            return;
        }

        if (m_actorTargets.TryGetValue(actor, out CameraTarget target))
        {
            m_currentTarget = target;
            Debug.Log($"Camera target set to: {actor.name}");
        }
        else
        {
            Debug.LogWarning($"No camera target found for actor: {actor.name}");
        }
    }

    private void RefreshActorTargets()
    {
        m_actorTargets.Clear();
        var targets = GameObject.FindObjectsByType<CameraTarget>(FindObjectsSortMode.None);

        Debug.Log($"CombatCameraMode: Searching for CameraTarget components...");
        
        foreach (var target in targets)
        {
            if (target.Actor != null)
            {
                m_actorTargets[target.Actor] = target;
                Debug.Log($"  - Found target for: {target.Actor.name}");
            }
            else
            {
                Debug.LogWarning($"  - CameraTarget on {target.gameObject.name} has no CombatActor parent!");
            }
        }

        Debug.Log($"CombatCameraMode: Found {m_actorTargets.Count} camera targets total");
    }

    private CameraPresetType DeterminePresetFromState()
    {
        // Priority: Check combat state first (Transition, Action)
        if (m_currentCombatState == CombatState.Transition)
        {
            return CameraPresetType.Transition;
        }
        
        if (m_currentCombatState == CombatState.Action)
        {
            // During action, use animation-controlled camera
            m_isAnimationControlled = true;
            return CameraPresetType.ActionExecution;
        }

        // If we have a current actor, check their state
        if (m_currentActor != null)
        {
            // Enemy turn - use single preset
            if (!m_currentActor.IsPlayer)
            {
                return CameraPresetType.EnemyTurn;
            }

            // Player turn - differentiate by actor state
            switch (m_currentActorState)
            {
                case CombatActorState.ActionSelecting:
                    return CameraPresetType.PlayerActionSelecting;
                    
                case CombatActorState.Targeting:
                    return CameraPresetType.PlayerTargeting;
            }
        }

        // Fallback
        return CameraPresetType.TurnStart;
    }

    public CameraPresetType CurrentPresetType => m_currentPresetType;

    #region Event Handlers

    private void OnTurnStarted(CombatActor actor)
    {
        Debug.Log($"CombatCameraMode: OnTurnStarted - {actor.name} (IsPlayer: {actor.IsPlayer})");
        
        m_currentActor = actor;
        SetTarget(actor);
        
        // Apply appropriate preset based on who's turn it is
        if (actor.IsPlayer)
        {
            ApplyPreset(CameraPresetType.TurnStart);
        }
        else
        {
            ApplyPreset(CameraPresetType.EnemyTurn);
        }
    }

    private void OnTurnEnded(CombatActor actor)
    {
        Debug.Log($"CombatCameraMode: OnTurnEnded - {actor.name}");
    }

    private void OnCombatStateChanged(CombatState state)
    {
        Debug.Log($"CombatCameraMode: OnCombatStateChanged - {state}");
        m_currentCombatState = state;
        
        var preset = DeterminePresetFromState();
        ApplyPreset(preset);
    }

    private void OnActorStateChanged(CombatActor actor, CombatActorState state)
    {
        // Only react to the current actor's state changes
        if (actor != m_currentActor) return;
        
        Debug.Log($"CombatCameraMode: OnActorStateChanged - {actor.name}: {state}");
        m_currentActorState = state;
        
        // Only apply preset if it's a player (enemies use single preset)
        if (actor.IsPlayer)
        {
            var preset = DeterminePresetFromState();
            ApplyPreset(preset);
        }
    }

    private void OnActionSubmitted(ActionContext ctx)
    {
        Debug.Log($"CombatCameraMode: OnActionSubmitted - {ctx.Source.name}");
        SetTarget(ctx.Source);
        // Combat state change will trigger ActionExecution preset
    }

    private void OnActionResolved(ActionContext ctx, ActionResult result)
    {
        Debug.Log("CombatCameraMode: OnActionResolved");
        m_isAnimationControlled = false;
    }

    private void OnTransitionStarted()
    {
        Debug.Log("CombatCameraMode: OnTransitionStarted");
        // Combat state change will trigger Transition preset
    }

    private void OnTransitionEnded()
    {
        Debug.Log("CombatCameraMode: OnTransitionEnded");
    }

    private void OnCombatActorsChanged(List<CombatActor> actors)
    {
        Debug.Log($"CombatCameraMode: OnCombatActorsChanged - {actors.Count} actors");
        RefreshActorTargets();
    }

    private void OnCombatEnded(CombatResult result)
    {
        Debug.Log($"CombatCameraMode: OnCombatEnded - {result}");
        ApplyPreset(CameraPresetType.CombatEnd);
    }

    private void OnAnimationTargetChanged(CameraTarget target)
    {
        m_currentTarget = target;
        Debug.Log($"CombatCameraMode: Animation target changed to {target.Actor.name}");
    }

    private void OnAnimationZoomChanged(float zoom)
    {
        if (m_currentTarget != null)
        {
            m_currentTarget.zoomMultiplier = zoom;
            Debug.Log($"CombatCameraMode: Zoom changed to {zoom}");
        }
    }

    #endregion
}