using Sandbox;
using System;

// default on-ground movement state.
// handles walking, friction, and jumping. resets all air-use flags (HasAirDashed, HasDoubleJumped, HasWallJumped) on enter.
// applies source-style ground friction via velocity decay each frame.
// transitions: jump -> AirborneState | hold jump -> MantleState | crouch + speed -> SlideState | crouch -> CrouchState | off ledge -> AirborneState.
public class GroundedState : BaseState
{
    public GroundedState( Movement manager ) : base( manager ) { }

    public override void Enter()
    {
        // refund air movement
        Manager.HasAirDashed = false;
        Manager.HasDoubleJumped = false;
        Manager.HasWallJumped = false;
    }

    public override BaseState Update()
    {
        Vector3 wishDir = GetWishDir();
        Vector3 targetVelocity = Manager.Controller.Velocity;

        // apply friciton
        targetVelocity = Vector3.Lerp( targetVelocity, Vector3.Zero, Manager.GroundFriction * Time.Delta );

        // calculate speed dot product and budget
        float currentSpeed = Vector3.Dot( targetVelocity, wishDir );
        float speedBudget = Manager.Speed - currentSpeed;

        if ( speedBudget > 0 )
        {
            // calculate accel and clamp it
            float accelAmount = Math.Min( Manager.GroundAcceleration * Manager.Speed * Time.Delta, speedBudget );
            targetVelocity += wishDir * accelAmount;
        }

        // zero out gravity while grounded
        targetVelocity.z = 0;        

        Manager.Controller.Velocity = targetVelocity;


        // --- transitions ---

        // dash
        if ( Input.Pressed( "run" ) && !Input.AnalogMove.IsNearlyZero() && Manager.Stamina.TryConsume( 1f ) )
        {
            return new DashState( Manager );
        }


        // jump
        if ( Input.Pressed( "jump" ) )
        {
            Manager.Controller.Punch( Vector3.Up * Manager.JumpForce );
        }

        // mantle check (carried jump input)
        if ( Input.Down( "jump" ) && !Input.Pressed( "jump" ) && Manager.TryGetMantleTarget( out Vector3 mantleTarget ) )
        {
            return new MantleState( Manager, mantleTarget );
        }

        // calculate slide threshold based on slope direction
        var groundTrace = Manager.Controller.TraceDirection( Vector3.Down * 2f );
        Vector3 downhillDir = groundTrace.Normal.WithZ(0).Normal;
        float slopeDot = Vector3.Dot( Manager.Controller.Velocity.WithZ(0).Normal, downhillDir );
        float currentSlideThreshold = Manager.MinSlideSpeed;

        if ( slopeDot > 0.17f ) // 80 degrees either side of straight downhill
        {
            // instantly drop threshold to 0 if within the downhill cone
            currentSlideThreshold = 0f;
        }

        // slide (explicitly requires the button to be PRESSED this frame)
        if ( Input.Pressed( "crouch" ) && Manager.Controller.Velocity.Length > currentSlideThreshold )
        {
            return new SlideState( Manager );
        }
        // crouch (fallback if just holding the button)
        else if ( Input.Down( "crouch" ) )
        {
            return new CrouchState( Manager );
        }

        // if no longer on the ground transition to AirborneState
        if ( !Manager.Controller.IsOnGround )
        {
            return new AirborneState( Manager );
        }

        // stay standing
        this.WishDir = wishDir;
        return null;
    }
}
