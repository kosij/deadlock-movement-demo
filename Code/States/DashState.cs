using Sandbox;
using System;

// short-duration directional burst. locks velocity to a fixed dash speed for DashDuration seconds.
// can only trigger once per air phase (HasAirDashed). captures input direction on entry as the locked dash vector.
// jumping within DashJumpWindow seconds of the dash ending applies a DashJumpForce vertical bonus.
// transitions: duration elapsed -> AirborneState | land during dash -> GroundedState or SlideState.
public class DashState : BaseState
{
    private TimeSince TimeSinceEntered;
    private Vector3 DashVelocity;
    private float DashDuration;
    private bool DashJumpLockedOut = false;

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

        // --- transitions ---

        // allow slide interrupt if crouch is explicitly PRESSED mid-dash
        if ( Manager.Controller.IsOnGround && Input.Pressed( "crouch" ) && DashVelocity.Length > Manager.MinSlideSpeed )
        {
            return new SlideState( Manager );
        }

        // dash-jump window
        // if jump is pressed before the window opens, lock it out
        if ( Input.Pressed( "jump" ) && TimeSinceEntered < DashDuration * Manager.DashJumpWindow )
        {
            DashJumpLockedOut = true;
            Log.Info( $"Dash Jump: TOO EARLY ({TimeSinceEntered:F3}s / window opens at {DashDuration * Manager.DashJumpWindow:F3}s)" );
        }
        // if successful dash-jump timing:
        if ( Manager.Controller.IsOnGround && !DashJumpLockedOut && TimeSinceEntered > DashDuration * Manager.DashJumpWindow )
        {
            if ( Input.Pressed( "jump" ) && Manager.Stamina.TryConsume( 1f ) )
            {
                Log.Info( "DASH SUCCESSFUL" );
                Manager.Controller.Velocity = DashVelocity.WithZ( 0 ) * 1.2f; // tuned to match reference absolute velocity
                Manager.Controller.Punch( Vector3.Up * Manager.DashJumpForce );
                return new AirborneState( Manager );
            }
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
