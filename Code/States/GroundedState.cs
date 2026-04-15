using Sandbox;
using System;

public class GroundedState : BaseState
{
    public GroundedState( Movement manager ) : base( manager ) { }

    public override BaseState Update()
    {
        // local vars and camera
        Vector3 wishDir = Input.AnalogMove;
        wishDir *= Manager.Scene.Camera.WorldRotation;
        wishDir.z = 0;
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
        // jump
        if ( Input.Pressed( "jump" ) )
        {
            Manager.Controller.Punch( Vector3.Up * Manager.JumpForce );
        }
        // crouch held down
        if ( Input.Down( "crouch" ) )
        {
            // fast enough -> slide
            if ( Manager.Controller.Velocity.Length > Manager.MinSlideSpeed)
            {
                return new SlideState( Manager );
            }
            // crouch walk
            else
            {
                return new CrouchState( Manager );
            }
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
