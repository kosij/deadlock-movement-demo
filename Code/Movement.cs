using Sandbox;
using System;
using Sandbox.Citizen;

public sealed class Movement : Component
{
    [Property] public float Speed { get; set; } = 300f;
    [Property] public float Gravity { get; set; } = 800f;
    [Property] public float JumpForce { get; set; } = 300f;
    [Property] public float AirJumpForce { get; set; } = 500f;
    [Property] public float GroundFriction { get; set; } = 4f;
    [Property] public float GroundAcceleration { get; set; } = 10f;
    [Property] public float AirSpeed { get; set; } = 30f;
    [Property] public float AirAcceleration { get; set; } = 100f;
    [Property] public float SlideFriction { get; set; } = 0.5f;
    [Property] public float MinSlideSpeed { get; set; } = 150f;
    [Property] public float CrouchSpeed { get; set; } = 100f;
    [Property] public float SlideSpeed { get; set; } = 50f;
    [Property] public float SlideAcceleration { get; set; } = 50f;
    [Property] public float GroundDashSpeed { get; set; } = 579f;
    [Property] public float AirDashSpeed { get; set; } = 527f;
    [Property] public float GroundDashDuration { get; set; } = 0.3f;
    [Property] public float AirDashDuration { get; set; } = 0.2f;
    [Property] public float AirDrag { get; set; } = 2f;
    [Property] public float DashJumpForce { get; set; } = 560f;
    [Property] public float DashJumpWindow { get; set; } = 0.7f;
    [Property] public float WallJumpForce { get; set; } = 400f;
    [Property] public float WallJumpKickForce { get; set; } = 300f;
    [Property] public float WallJumpInputBoost { get; set; } = 200f;
    [Property] public float WallCoyoteTime { get; set; } = 0.15f;

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