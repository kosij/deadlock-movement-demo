using Sandbox;
using System;

// slow ground movement while crouching. reduces controller height on enter and restores it on exit.
// uses the same source-style acceleration as GroundedState but capped at CrouchSpeed.
// transitions: release crouch -> GroundedState | off ledge -> AirborneState | dash -> DashState.
public class CrouchState : BaseState
{
    public CrouchState( Movement manager ) : base( manager ) { }

    public override void Enter()
    {
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
        targetVelocity = Vector3.Lerp( targetVelocity, Vector3.Zero, Manager.GroundFriction * Time.Delta );

        // calculate speed dot product and budget
        float currentSpeed = Vector3.Dot( targetVelocity, wishDir );
        float speedBudget = Manager.CrouchSpeed - currentSpeed; 

        if ( speedBudget > 0 )
        {
            // calculate accel and clamp it
            float accelAmount = Math.Min( Manager.GroundAcceleration * Manager.CrouchSpeed * Time.Delta, speedBudget );
            targetVelocity += wishDir * accelAmount;
        }

        // zero out gravity
        targetVelocity.z = 0;        

        // set velocity
        Manager.Controller.Velocity = targetVelocity;

        // --- transitions ---

        // dash
        if ( Input.Pressed( "run" ) && !Input.AnalogMove.IsNearlyZero() )
        {
            return new DashState( Manager );
        }
        
        // airborne
        if ( !Manager.Controller.IsOnGround ) return new AirborneState( Manager );

        // un-crouch
        if ( !Input.Down( "crouch" ) ) return new GroundedState( Manager );

        
        // stay in Crouch State
        this.WishDir = wishDir;
        return null;
    }

    public override void Exit()
    {
        // restore height (stand up)
        Manager.Controller.Height = 64f;
    }
}
