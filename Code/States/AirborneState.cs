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

        // normalise vector
        wishDir = wishDir.Normal;
        
        Vector3 targetVelocity = Manager.Controller.Velocity;


        // calculate speed dot product and budget
        float currentSpeed = Vector3.Dot( targetVelocity, wishDir );
        float speedBudget = Manager.AirSpeed - currentSpeed;

        if ( speedBudget > 0 )
        {
            float accelAmount = Math.Min( Manager.AirAcceleration * Manager.AirSpeed * Time.Delta, speedBudget );
            targetVelocity += wishDir * accelAmount;
        }

        // soft-cap air drag
        Vector3 lateralVelocity = targetVelocity.WithZ( 0 );
        if ( lateralVelocity.Length > 300 )
        {
            lateralVelocity = Vector3.Lerp( lateralVelocity, lateralVelocity.Normal * Manager.AirSpeed, Manager.AirDrag * Time.Delta );
            targetVelocity = lateralVelocity.WithZ( targetVelocity.z );
        }

        // add gravity
        targetVelocity.z -= Manager.Gravity * Time.Delta;
        
        // jump
        if ( Input.Pressed( "jump" ) && !Manager.HasDoubleJumped )
        {
            targetVelocity.z = Manager.AirJumpForce;
            Manager.HasDoubleJumped = true;
        }

        Manager.Controller.Velocity = targetVelocity;

        // --- transitions ---

        // dash
        if ( Input.Pressed( "run" ) && !Manager.HasAirDashed && !Input.AnalogMove.IsNearlyZero() )
        {
            return new DashState( Manager );
        }

        // landed
        if ( Manager.Controller.IsOnGround )
        {
            // slide queuing (if they hold crouch while falling)
            // ommiting the z axis so fall speed doesn't factor into slide threshold check
            if ( Input.Down( "crouch" ) && Manager.Controller.Velocity.WithZ(0).Length > Manager.MinSlideSpeed )
            {
                return new SlideState( Manager );
            }

            return new GroundedState( Manager );
        }

        // wall slide (detect walls)
        bool touchingWall = false;
        Vector3 center = Manager.Transform.Position + Vector3.Up * 32f; // middle of the player
        for ( int i = 0; i < 8; i++ )
        {
            Vector3 dir = Rotation.FromYaw( i * 45f ).Forward;
            var hit = Manager.Scene.Trace.Ray( center, center + dir * 24f )
                .IgnoreGameObjectHierarchy( Manager.GameObject )
                .Run();

            if ( hit.Hit && Math.Abs( hit.Normal.z ) < 0.1f )
            {
                touchingWall = true;
                break; // just checking if we are touching a wall
            }
        }

        if ( touchingWall )
        {
            return new WallSlideState( Manager );
        }

        // stay airborne
        this.WishDir = wishDir;
        return null;
    }
}
