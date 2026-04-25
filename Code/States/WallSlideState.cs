using Sandbox;
using System;
using System.Linq;

public class WallSlideState : BaseState
{
    public WallSlideState( Movement manager ) : base( manager ) { }

    public override void Enter()
    {
        
    }

    public override BaseState Update()
    {
        // local vars and camera
        Vector3 wishDir = Input.AnalogMove;
        wishDir *= Manager.Scene.Camera.WorldRotation;
        wishDir.z = 0;
        wishDir = wishDir.Normal;

        Vector3 targetVelocity = Manager.Controller.Velocity;

        // maintain air speed and drag like AirborneState
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

        // fall with normal gravity
        targetVelocity.z -= Manager.Gravity * Time.Delta;

        // check for wall contact using 8-way raycast
        bool touchingWall = false;
        Vector3 wallNormalSum = Vector3.Zero;
        Vector3 center = Manager.Transform.Position + Vector3.Up * 32f;

        for ( int i = 0; i < 8; i++ )
        {
            Vector3 dir = Rotation.FromYaw( i * 45f ).Forward;
            var hit = Manager.Scene.Trace.Ray( center, center + dir * 24f )
                .IgnoreGameObjectHierarchy( Manager.GameObject )
                .Run();

            if ( hit.Hit && Math.Abs( hit.Normal.z ) < 0.1f )
            {
                touchingWall = true;
                wallNormalSum += hit.Normal;
            }
        }

        // wall Jump logic
        if ( touchingWall && Input.Pressed( "jump" ) )
        {
            Vector3 averageNormal = wallNormalSum.Normal;

            // wipe vertical momentum
            targetVelocity.z = 0f;

            // wipe momentum in the normal direction
            // still preserving parallel momentum
            float dot = Vector3.Dot( targetVelocity, averageNormal );
            targetVelocity -= averageNormal * dot;

            // apply kick force
            targetVelocity += averageNormal * Manager.WallJumpKickForce;

            // apply vertical jump force (penalty if consecutive)
            if ( !Manager.HasWallJumped )
            {
                targetVelocity.z += Manager.WallJumpForce;
                Manager.HasWallJumped = true;
            }

            // apply input influence burst ( input direction influences the wall jump vector )
            targetVelocity += wishDir * Manager.WallJumpInputBoost;

            Manager.Controller.Velocity = targetVelocity;
            
            return new AirborneState( Manager );
        }

        Manager.Controller.Velocity = targetVelocity;

        // --- transitions ---

        // dash
        if ( Input.Pressed( "run" ) && !Manager.HasAirDashed && !Input.AnalogMove.IsNearlyZero() )
        {
            return new DashState( Manager );
        }

        if ( Manager.Controller.IsOnGround )
        {
            if ( Input.Down( "crouch" ) && Manager.Controller.Velocity.WithZ(0).Length > Manager.MinSlideSpeed )
            {
                return new SlideState( Manager );
            }
            return new GroundedState( Manager );
        }

        if ( !touchingWall )
        {
            Manager.LastWallNormal = wallNormalSum.Normal;
            Manager.TimeSinceLeftWall = 0;
            return new AirborneState( Manager );
        }

        this.WishDir = wishDir;
        return null;
    }
}
