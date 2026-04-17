using Sandbox;
using System;

public class DashState : BaseState
{
    private TimeSince TimeSinceEntered;
    private Vector3 DashVelocity;
    private float DashDuration;

    public DashState( Movement manager ) : base( manager ) { }

    public override void Enter()
    {
        TimeSinceEntered = 0;

        // calculate base direction
        Vector3 dashDir = Input.AnalogMove;
        
        dashDir *= Manager.Scene.Camera.WorldRotation;
        dashDir.z = 0;
        dashDir = dashDir.Normal;

        // select speed based on if grounded or airborne
        float burstSpeed = Manager.Controller.IsOnGround ? Manager.GroundDashSpeed : Manager.AirDashSpeed;
        DashDuration = Manager.Controller.IsOnGround ? Manager.GroundDashDuration : Manager.AirDashDuration;
        
        // lock in velocity
        DashVelocity = dashDir * burstSpeed;
        Manager.Controller.Velocity = DashVelocity;

        // if airborne we consume our air dash (only 1 air dash allowed)
        if ( !Manager.Controller.IsOnGround )
        {
            Manager.HasAirDashed = true;
        }
    }

    public override BaseState Update()
    {
        // lock momentum to ignore friction/gravity
        Manager.Controller.Velocity = DashVelocity;

        // allow slide interrupt if crouch is explicitly PRESSED mid-dash
        if ( Manager.Controller.IsOnGround && Input.Pressed( "crouch" ) && DashVelocity.Length > Manager.MinSlideSpeed )
        {
            return new SlideState( Manager );
        }

        // don't allow player to interrupt a dash with another dash or jump.
        // wait for duration to expire
        if ( TimeSinceEntered > DashDuration )
        {
            // exit dash and gracefully hand momentum back to airborne or grounded
            if ( Manager.Controller.IsOnGround )
            {
                // -> grounded state
                return new GroundedState( Manager );
            }
            else
            {
                // -> air state
                return new AirborneState( Manager );
            }
        }

        this.WishDir = DashVelocity.Normal;
        return null;
    }
}
