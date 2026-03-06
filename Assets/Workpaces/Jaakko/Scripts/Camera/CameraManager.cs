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
    }    
    public bool Init() 
    {
        m_cinemachineCamera = GameObject
            .FindFirstObjectByType<CinemachineCamera>();
        return true;
    }
    public bool Dispose() 
    {
        return true;
    }
    public void OnManagersInitialzied() 
    {
        m_container = new Container();
        m_container.RegisterInstance(this);
        m_container.RegisterInstance(m_actorManager);
        m_container.RegisterInstance(m_combatManager);
        m_container.RegisterInstance(m_game);

        m_container.Register<FollowActorCameraMode>();
        m_container.Register<CombatCameraMode>();

        m_cameraModes = m_container.GetAll<ICameraMode>().ToList();

        var defaultMode = m_cameraModes.FirstOrDefault();
        if (defaultMode != null)
            SetMode(defaultMode);
    }
    public void Update(float dt)
    {
        m_mode.Update(dt);

        var nextMode = m_cameraModes.FirstOrDefault(m => m.CanEnter());
        if (nextMode != null && nextMode != m_mode)
        {
            SetMode(nextMode);
        }

        // Update current mode
        m_mode?.Update(dt);
    }
    private void SetMode(ICameraMode mode) 
    {
        m_mode?.Exit();
        m_mode = mode;
        m_mode.Enter();
    }
}
