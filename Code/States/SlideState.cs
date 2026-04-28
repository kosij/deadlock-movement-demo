using Sandbox;
using System;

// low-friction ground slide. entered from GroundedState or DashState when moving above MinSlideSpeed while crouching.
// preserves and builds horizontal momentum via reduced friction. on slopes, injects gravity-derived horizontal acceleration
// to keep the player accelerating downhill even when no input is held.
// transitions: speed below MinSlideSpeed -> CrouchState | jump -> AirborneState.
public class SlideState : BaseState
{
    public SlideState( Movement manager ) : base( manager ) { }

    public override void Enter()
    {
        // refund air movement
        Manager.HasAirDashed = false;
        Manager.HasDoubleJumped = false;
        Manager.HasWallJumped = false;
        // crouch camera height
        Manager.Controller.Height = 36f;
    }

    public override BaseState Update()
    {
        // local vars and camera
        Vector3 wishDir = Input.AnalogMove;
        wishDir *= Manager.Scene.Camera.WorldRotation;
        wishDir.z = 0;
        // normalise vector
        wishDir = wishDir.Normal;
        Vector3 targetVelocity = Manager.Controller.Velocity;

        // calculate slide threshold based on slope direction
        var groundTrace = Manager.Controller.TraceDirection( Vector3.Down * 2f );
        Vector3 downhillDir = groundTrace.Normal.WithZ(0).Normal;
        float slopeDot = Vector3.Dot( targetVelocity.WithZ(0).Normal, downhillDir );
        
        bool isMovingDownhill = slopeDot > 0.17f; // 80 degrees either side of straight downhill

        if ( isMovingDownhill )
        {
            // accelerate down the slope using gravity
            // the steeper the slope (Normal.z gets closer to 0), the more gravity applies
            float slopeSteepness = 1f - groundTrace.Normal.z;
            float gravityPull = 800f;
            
            targetVelocity += downhillDir * (gravityPull * slopeSteepness * Time.Delta);
        }
        else
        {
            // apply friction (no friction if sliding downhill)
            targetVelocity = Vector3.Lerp( targetVelocity, Vector3.Zero, Manager.SlideFriction * Time.Delta );
        }

        // calculate speed dot product and budget
        float currentSpeed = Vector3.Dot( targetVelocity, wishDir );
        float speedBudget = Manager.SlideSpeed - currentSpeed;

        if ( speedBudget > 0 )
        {
            float accelAmount = Math.Min( Manager.SlideAcceleration * Manager.SlideSpeed * Time.Delta, speedBudget );
            targetVelocity += wishDir * accelAmount;
        }

        // zero out gravity
        targetVelocity.z = 0;        

        // set velocity
        Manager.Controller.Velocity = targetVelocity;

        // --- transitions ---
        
        // jump
        if ( Input.Pressed( "jump" ) )
        {
            Manager.Controller.Punch( Vector3.Up * Manager.JumpForce );
            return new AirborneState( Manager );
        }

        // airborne
        if ( !Manager.Controller.IsOnGround ) return new AirborneState( Manager );

        // un-crouch
        if ( !Input.Down( "crouch" ) ) return new GroundedState( Manager );

        // too slow (0 threshold if moving downhill)
        float currentExitThreshold = isMovingDownhill ? 0f : Manager.CrouchSpeed;
        if ( targetVelocity.Length < currentExitThreshold ) return new CrouchState( Manager );

        // stay in Slide State
        this.WishDir = wishDir;
        return null;
    }

    public override void Exit()
    {
        // restore height (stand up)
        Manager.Controller.Height = 64f;
    }
}
