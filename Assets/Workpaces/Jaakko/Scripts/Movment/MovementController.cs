using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
/*
public struct InputState
{
    public Vector2 InputDir;
    public bool JumpPressed;
    public bool JumpJustPressed;
    public bool DashPressed;
    public bool DashPressedThisFrame;
    public bool CrouchPressed;
}
*/
public struct MovmentState
{
    public bool IsGrounded;
    public Vector3 ContactNormal;
    public bool OnWall;
    public bool IsSliding;
}

[RequireComponent(typeof(CharacterController))]
public class MovementController : MonoBehaviour, IActorComponent
{
    private CharacterController m_controller;
    private InputState m_inputState;
    public InputState InputState => m_inputState;

    private MovmentState m_movmentState;
    public MovmentState MovmentState => m_movmentState; 

    public Vector3 Velocity { get; set; }

    public Transform Player => transform;

    private List<IEnvironmentModule> environmentModules = new();
    private List<IIntentModule> intentModules = new();
    private List<IImpulseModule> impulseModules = new();
    private List<IForceModule> forceModules = new();
    private List<IPostProcessModule> postProcessModules = new();

    [Header("RayCast")]
    [SerializeField] private LayerMask m_defaultLayer;

    [Header("TempVisuals")]
    [SerializeField] private GameObject m_visualCapsule;

    public event Action<Vector3> OnMove;

    [SerializeField] private PlayerStats m_playerStats;
    public PlayerStats RuntimeStats => m_playerStats;
    public void Move(Vector3 pos) 
    {
        m_controller.enabled = false;
        Velocity = Vector3.zero;
        transform.position = pos;
        m_controller.enabled = true;
    }
    public void LoadData(ActorSaveData data)
    {
        m_controller.enabled = false;
        transform.position = data.Position;
        m_controller.enabled = true;
    }
    public void MoveTo(Transform t) 
    {
        m_controller.enabled = false;
        transform.position = t.position;
        transform.rotation = t.rotation;
        m_controller.enabled = true;
    }
    public void SaveData(ActorSaveData data)
    {

    }
    public bool Initialize(GameManager game)
    {
        m_controller = GetComponent<CharacterController>();

        AddModule(new WalkRunModule(this));
        AddModule(new GroundFrictionModule(this));
        return true;
    }    
    public bool Dispose()
    {
        return true;
    }
    public void OnActorComponentsInitialized(Actor actor)
    {

    }
    private IInputSource m_inputSource;
    public void SetInputSource(IInputSource source)
    {
        m_inputSource = source;
    }

    private void AddModule(object module)
    {
        if (module is IEnvironmentModule env) environmentModules.Add(env);
        if (module is IIntentModule intent) intentModules.Add(intent);
        if (module is IImpulseModule imp) impulseModules.Add(imp);
        if (module is IForceModule force) forceModules.Add(force);
        if (module is IPostProcessModule post) postProcessModules.Add(post);
    }    
    private void Update()
    {
        if (m_inputSource == null) return;

        m_inputState = m_inputSource.GetInput();

        UpdateContact();
        CollisionFlags flags = m_controller.Move(Velocity * Time.deltaTime);
        OnMove?.Invoke(Velocity);

        for (int i = 0; i < environmentModules.Count; i++)
        {
            environmentModules[i].UpdateEnviroment();
        }
        for (int i = 0; i < intentModules.Count; i++)
        {
            intentModules[i].UpdateIntent();
        }                
        for (int i = 0; i < impulseModules.Count; i++)
        {
            impulseModules[i].UpdateImpulse();
        }
        for (int i = 0; i < forceModules.Count; i++)
        {
            forceModules[i].UpdateForce();
        }
        for (int i = 0; i < postProcessModules.Count; i++)
        {
            postProcessModules[i].UpdatePostProcess();
        }
    }
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {        
        m_movmentState.ContactNormal = hit.normal;

        float slope = Vector3.Angle(hit.normal, Vector3.up);
        bool isSteep = slope > 45f; // This should be a parameter in stats

        bool notGrounded = !m_movmentState.IsGrounded;

        bool movingIntoWall = Vector3.Dot(Velocity, -hit.normal) > 0f;

        m_movmentState.OnWall = isSteep && notGrounded && movingIntoWall;        
    }
    public void SetSliding(bool value) 
    {
        m_movmentState.IsSliding = value;    
        
        if (value) 
        {
            m_visualCapsule.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
        }
        else        
        {
            m_visualCapsule.transform.localRotation = Quaternion.identity;
        }
    }

    private void UpdateContact()
    {
        m_movmentState.IsGrounded = m_controller.isGrounded;
        m_movmentState.OnWall = false;

        if (!m_movmentState.IsGrounded)
        {
            GroundedRayCheck();
        }
    }
    private void GroundedRayCheck() 
    {
        float rayLength = 0.1f;
        Vector3 origin = transform.position + m_controller.center;

        if (Physics.SphereCast(origin, m_controller.radius * 0.9f, Vector3.down,
            out RaycastHit hit, m_controller.height / 2 + rayLength)) 
        {
            float slope = Vector3.Angle(hit.normal, Vector3.up);
            if (slope <= 45f) // This should be a parameter in stats 
            {
                m_movmentState.IsGrounded = true;
                m_movmentState.ContactNormal = hit.normal;
            }
        }
    }
    private void OnDrawGizmos()
    {
        /*
        if (m_controller == null || m_cameraManager == null) return;

        Vector3 origin = transform.position + m_controller.center;
        Vector3 normal = m_movmentState.ContactNormal;

        // Contact normal
        if (normal.sqrMagnitude > 0.0001f)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(origin, normal * 2f);
        }

        // Input direction (world space)
        Vector3 camForward = m_cameraManager.transform.forward;
        Vector3 camRight = m_cameraManager.transform.right;

        camForward.y = 0f;
        camRight.y = 0f;

        Vector3 moveDir =
            camForward.normalized * m_inputState.InputDir.y +
            camRight.normalized * m_inputState.InputDir.x;

        if (moveDir.sqrMagnitude > 0.0001f)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(origin, moveDir * 2f);
        }
        */
    }
}
