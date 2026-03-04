using UnityEngine;

[CreateAssetMenu(menuName = "Stats/Movement Stats")]
public class MovementStats : ScriptableObject
{
    [Header("Ground")]
    public float WalkSpeed = 5f;
    public float RunSpeed = 8f;
    public float Acceleration = 10f;
    public float Deceleration = 14f; 
    public float JumpForce = 5f;
    public float StrafeControlFactor = 0.6f;

    [Header("Movement")]
    public float TimeToRun = 0.3f;

    [Header("Air")]
    public float MaxAirSpeed = 6f;
    public float AirAcceleration = 20f;
    public float AirControl = 1f;

    [Header("Enviroment")]
    public float Gravity = -9.81f;
    public float MaxSlopeAngle = 45f;    

    [Header("GroundFriction")]
    public float TimeToStop = 0.2f;
    public float SlideFactor = 0.6f;
    public float BrakeFactor = 8f;

    [Header("Stamina")]
    public float MaxStamina = 5f;
    public float StaminaRegenRate = 1f;

    [Header("Dash")]
    public float DashDistance = 3f;
    public float DashTime = 0.2f;
    public float DashStaminaCost = 1f;
    public float DashCooldown = 0.5f;

    [Header("Jump")]
    public int MaxAirJumps = 1;

    [Header("WallJump")]
    public float WallJumpDistance = 8f;
    public float WallJumpInputInfluence = 2f;
    public float WallJumpHeight = 6f;
    
    public float WallJumpStaminaCost = 0.25f;

}
