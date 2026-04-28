using Sandbox;
using System;
using Sandbox.Citizen;

public sealed class Movement : Component
{
    // property units: u = source world units | s = seconds | mult = dimensionless multiplier

    // --- Physics ---
    [Property] public float Gravity { get; set; } = 800f;            // u/s² | gravity acceleration

    // --- Ground ---
    [Property] public float Speed { get; set; } = 300f;              // u/s  | ground movement speed
    [Property] public float JumpForce { get; set; } = 300f;          // u/s  | vertical jump velocity
    [Property] public float GroundFriction { get; set; } = 4f;       // mult | deceleration multiplier
    [Property] public float GroundAcceleration { get; set; } = 10f;  // mult | acceleration multiplier

    // --- Air ---
    [Property] public float AirJumpForce { get; set; } = 500f;       // u/s  | vertical velocity (double jump)
    [Property] public float AirSpeed { get; set; } = 30f;            // u/s  | lateral air speed cap
    [Property] public float AirAcceleration { get; set; } = 100f;    // mult | air strafe acceleration
    [Property] public float AirDrag { get; set; } = 2f;              // mult | drag applied above AirSpeed cap
    [Property] public float WallJumpInputBoost { get; set; } = 200f; // u/s  | directional impulse on air jumps

    // --- Slide ---
    [Property] public float MinSlideSpeed { get; set; } = 150f;      // u/s  | minimum speed to enter slide
    [Property] public float SlideFriction { get; set; } = 0.5f;      // mult | friction multiplier while sliding
    [Property] public float SlideSpeed { get; set; } = 50f;          // u/s  | speed added on slide entry
    [Property] public float SlideAcceleration { get; set; } = 50f;   // u/s² | downhill slide push acceleration
    [Property] public float CrouchSpeed { get; set; } = 100f;        // u/s  | max speed while crouching

    // --- Dash ---
    [Property] public float GroundDashSpeed { get; set; } = 579f;    // u/s  | ground dash velocity
    [Property] public float AirDashSpeed { get; set; } = 527f;       // u/s  | air dash velocity
    [Property] public float GroundDashDuration { get; set; } = 0.3f; // s    | ground dash duration
    [Property] public float AirDashDuration { get; set; } = 0.2f;    // s    | air dash duration
    [Property] public float DashJumpForce { get; set; } = 560f;      // u/s  | vertical boost on dash-jump
    [Property] public float DashJumpWindow { get; set; } = 0.7f;     // s    | window after dash where jump is boosted

    // --- Wall Jump ---
    [Property] public float WallJumpForce { get; set; } = 400f;      // u/s  | vertical velocity on wall jump
    [Property] public float WallJumpKickForce { get; set; } = 300f;  // u/s  | lateral kick away from wall
    [Property] public float WallCoyoteTime { get; set; } = 0.15f;    // s    | coyote window after leaving wall

    public bool HasAirDashed { get; set; } = false;
    public bool HasDoubleJumped { get; set; } = false;
    public bool HasWallJumped { get; set; } = false;
    public TimeSince TimeSinceLeftWall;


    [RequireComponent] public CitizenAnimationHelper Animator { get; set; }
    [RequireComponent] public CharacterController Controller { get; set; }
    public BaseState CurrentState { get; private set; }

    protected override void OnStart()
    {
        CurrentState = new AirborneState( this );
        CurrentState.Enter();
    }

    protected override void OnUpdate()
    {
        BaseState nextState = CurrentState.Update();

        if ( nextState != null )
        {
            CurrentState.Exit();
            CurrentState = nextState;
            CurrentState.Enter();
        }

        // rotate player with camera
        WorldRotation = Rotation.FromYaw( Scene.Camera.WorldRotation.Angles().yaw );

        Controller.Move();
        UpdateAnimations();
    }

    private void UpdateAnimations()
    {
        if ( Animator == null ) return;

        // freeze the legs animation during dash and slide
        if ( CurrentState is SlideState || CurrentState is DashState )
        {
            Animator.WithVelocity( Vector3.Zero );
        }
        else
        {
            Animator.WithVelocity( Controller.Velocity );
        }

        Animator.WithWishVelocity( CurrentState.WishDir );
        Animator.IsGrounded = Controller.IsOnGround;

        if ( CurrentState is SlideState ) 
        {
            Animator.DuckLevel = 1f;
            Animator.IsSitting = true;
        }
        else if ( CurrentState is CrouchState )
        {
            Animator.DuckLevel = 1f;
            Animator.IsSitting = false;
        }
        else if ( CurrentState is WallSlideState )
        {
            // pretending we are grounded so we can use the crouch animation for wall slide
            Animator.IsGrounded = true;
            Animator.DuckLevel = 1f;
            Animator.IsSitting = false;
        }
        else 
        {
            Animator.DuckLevel = 0f;
            Animator.IsSitting = false;
        }
    }

}