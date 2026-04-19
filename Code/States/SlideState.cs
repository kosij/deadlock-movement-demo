using Sandbox;
using System;

public class SlideState : BaseState
{
    public SlideState( Movement manager ) : base( manager ) { }

    public override void Enter()
    {
        // refund air dash count 
        Manager.HasAirDashed = false;
        Manager.HasDoubleJumped = false;
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

        // apply friciton
        targetVelocity = Vector3.Lerp( targetVelocity, Vector3.Zero, Manager.SlideFriction * Time.Delta );

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

        // too slow
        if ( targetVelocity.Length < Manager.CrouchSpeed ) return new CrouchState( Manager );

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
