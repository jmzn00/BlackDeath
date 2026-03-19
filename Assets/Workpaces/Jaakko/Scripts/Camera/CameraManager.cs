using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : IManager
{
    private ActorManager m_actorManager;
    private CombatManager m_combatManager;
    private GameManager m_game;

    private Camera m_camera;
    private CinemachineCamera m_cinemachineCamera;
    public CinemachineCamera Camera => m_cinemachineCamera;

    private Container m_container;

    private List<ICameraMode> m_cameraModes;

    private ICameraMode m_mode;
    public CameraManager(ActorManager actorManager, CombatManager combatManager, GameManager gameManager) 
    {
        m_actorManager = actorManager;
        m_combatManager = combatManager;
        m_game = gameManager;
        //Debug.Log("CameraManager constructed");
    }    
    public bool Init() 
    {
        m_cinemachineCamera = GameObject.FindFirstObjectByType<CinemachineCamera>();
        if (m_cinemachineCamera == null)
        {
            Debug.LogError("CameraManager: No CinemachineCamera found in scene!");
            return false;
        }
        //Debug.Log($"CameraManager: Found CinemachineCamera: {m_cinemachineCamera.name}");
        return true;
    }
    public bool Dispose() 
    {
        return true;
    }
    public void OnManagersInitialzied() 
    {
        //Debug.Log("CameraManager: OnManagersInitialized");
        
        m_container = new Container();
        m_container.RegisterInstance(this);
        m_container.RegisterInstance(m_actorManager);
        m_container.RegisterInstance(m_combatManager);
        m_container.RegisterInstance(m_game);

        m_container.Register<FollowActorCameraMode>();
        m_container.Register<CombatCameraMode>();

        m_cameraModes = m_container.GetAll<ICameraMode>().ToList();
        //Debug.Log($"CameraManager: Registered {m_cameraModes.Count} camera modes");
        
        // Log which modes were created
        foreach (var mode in m_cameraModes)
        {
            //Debug.Log($"  - {mode.GetType().Name}");
        }

        var defaultMode = m_cameraModes.FirstOrDefault();
        if (defaultMode != null)
        {
            Debug.Log($"CameraManager: Setting default mode to {defaultMode.GetType().Name}");
            SetMode(defaultMode);
        }
        else
        {
            Debug.LogError("CameraManager: No camera modes available!");
        }
    }
    public void Update(float dt)
    {
        if (m_mode == null)
        {
            Debug.LogWarning("CameraManager: No active mode in Update");
            return;
        }

        var nextMode = m_cameraModes.FirstOrDefault(m => m.CanEnter());
        if (nextMode != null && nextMode != m_mode)
        {
            //Debug.Log($"Camera switching from {m_mode.GetType().Name} to {nextMode.GetType().Name}");
            SetMode(nextMode);
        }

        // Update current mode
        m_mode?.Update(dt);
    }
    private void SetMode(ICameraMode mode) 
    {
        if (m_mode != null)
        {
            Debug.Log($"CameraManager: Exiting {m_mode.GetType().Name}");
            m_mode.Exit();
        }
        
        m_mode = mode;
        //Debug.Log($"CameraManager: Entering {m_mode.GetType().Name}");
        m_mode.Enter();
    }
}
