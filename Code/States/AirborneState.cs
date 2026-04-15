using Sandbox;
using System;

public class AirborneState : BaseState
{
    public AirborneState( Movement manager ) : base( manager ) { }

    public override BaseState Update()
    {
        // local vars and camera
        Vector3 wishDir = Input.AnalogMove;
        wishDir *= Manager.Scene.Camera.WorldRotation;
        wishDir.z = 0;
        Vector3 targetVelocity = Manager.Controller.Velocity;


        // calculate speed dot product and budget
        float currentSpeed = Vector3.Dot( targetVelocity, wishDir );
        float speedBudget = Manager.AirSpeed - currentSpeed;

        if ( speedBudget > 0 )
        {
            float accelAmount = Math.Min( Manager.AirAcceleration * Manager.AirSpeed * Time.Delta, speedBudget );
            targetVelocity += wishDir * accelAmount;
        }

        // add gravity
        targetVelocity.z -= Manager.Gravity * Time.Delta;

        Manager.Controller.Velocity = targetVelocity;

        // --- transitions ---

        // landed
        if ( Manager.Controller.IsOnGround )
        {
            return new GroundedState( Manager );
        }

        // stay airborne
        this.WishDir = wishDir;
        return null;
    }
}
