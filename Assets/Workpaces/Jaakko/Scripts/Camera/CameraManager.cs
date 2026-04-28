using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : ManagerBase
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
    public override void OnSceneLoaded(SceneData data) 
    {
        IsReady = false;
        if (!data.IsGameplay) 
        {
            SetReady();
            return;
        }

        m_cinemachineCamera = GameObject.FindFirstObjectByType<CinemachineCamera>();
        if (m_cinemachineCamera == null)
        {
            Debug.LogError("CameraManager: No CinemachineCamera found in scene!");
        }

        CombatEvents.OnCombatStarted += OnCombatStarted;
        CombatEvents.OnCombatCameraEnded += OnCombatEnded;

        // Sorting mode so no transparency with sprites

        Camera unityCam = UnityEngine.Camera.main;
        unityCam.transparencySortMode = TransparencySortMode.CustomAxis;
        unityCam.transparencySortAxis = new Vector3(0f, 0f, 1f);

        var defaultMode = m_cameraModes.FirstOrDefault();
        if (defaultMode != null)
        {
            SetMode(defaultMode);
        }
        else
        {
            Debug.LogError("CameraManager: No camera modes available!");
        }        
        SetReady();
    }

    private void OnCombatStarted()
    {
        SetMode(m_cameraModes.FirstOrDefault(m => m is CombatCameraMode));
    }

    private void OnCombatEnded()
    {
        SetMode(m_cameraModes.FirstOrDefault(m => m is FollowActorCameraMode));
    }   

    public override bool Init() 
    {
        m_container = new Container();
        m_container.RegisterInstance(this);
        m_container.RegisterInstance(m_actorManager);
        m_container.RegisterInstance(m_combatManager);
        m_container.RegisterInstance(m_game);

        m_container.Register<FollowActorCameraMode>();
        m_container.Register<CombatCameraMode>();

        m_cameraModes = m_container.GetAll<ICameraMode>().ToList();

        return true;
    }
    public override void Update(float dt)
    {                
        if (m_mode == null)
        {
            return;
        }

        var nextMode = m_cameraModes.FirstOrDefault(m => m.CanEnter());
        if (nextMode != null && nextMode != m_mode)
        {
            SetMode(nextMode);
        }
        m_mode?.Update(dt);        
    }
    private void SetMode(ICameraMode mode) 
    {
        if (m_mode != null)
        {
            m_mode.Exit();
        }
        
        m_mode = mode;
        m_mode.Enter();
    }
}
