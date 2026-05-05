using Sandbox;
using System;

// aerial movement state. active whenever the player is not grounded.
// implements source-style air strafing: acceleration is only added when WishDir has a positive dot product with velocity,
// preventing uncapped speed gain and preserving the momentum-conservative feel.
// handles double jump, wall jump coyote window (TimeSinceLeftWall < WallCoyoteTime), and wall detection for WallSlideState.
// transitions: land -> GroundedState | wall contact -> WallSlideState | mantle -> MantleState | dash -> DashState.
public class AirborneState : BaseState
{
    public AirborneState( Movement manager ) : base( manager ) { }

    public override BaseState Update()
    {
        Vector3 wishDir = GetWishDir();

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
        
        // mantle check (held jump input)
        if ( Input.Down( "jump" ) && Manager.TryGetMantleTarget( out Vector3 mantleTarget, isAirborne: true ) )
            return new MantleState( Manager, mantleTarget );

        // wall-jump or double jump
        if ( Input.Pressed( "jump" ) )
        {
            if ( Manager.TimeSinceLeftWall < Manager.WallCoyoteTime )
            {
                // fire raycast starburst to find wall geometry near player's current position during wall coyote time
                // this naturally captures diagonal corner normals (average between 2 wall normals) for an accurate edge boost kick
                Vector3 coyoteCenter = Manager.Transform.Position + Vector3.Up * 32f;
                Vector3 wallNormalSum = Vector3.Zero;
                bool foundWall = false;

                for ( int i = 0; i < 8; i++ )
                {
                    Vector3 dir = Rotation.FromYaw( i * 45f ).Forward;
                    var hit = Manager.Scene.Trace.Ray( coyoteCenter, coyoteCenter + dir * 64f )
                        .IgnoreGameObjectHierarchy( Manager.GameObject )
                        .Run();

                    if ( hit.Hit && Math.Abs( hit.Normal.z ) < 0.1f )
                    {
                        wallNormalSum += hit.Normal;
                        foundWall = true;
                    }
                }

                // first wall jump is free; subsequent ones cost 0.5 stamina
                // short-circuit: TryConsume only called when HasWallJumped is true
                bool wallJumpAllowed = !Manager.HasWallJumped || Manager.Stamina.TryConsume( 0.5f );

                if ( foundWall && wallJumpAllowed )
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
                }
            }
            else if ( !Manager.HasDoubleJumped && Manager.Stamina.TryConsume( 1f ) )
            {
                targetVelocity.z = Manager.AirJumpForce;
                Manager.HasDoubleJumped = true;
                // apply input influence burst ( input direction influences the double jump vector )
                targetVelocity += wishDir * Manager.AirJumpInputBoost;
            }
        }

        Manager.Controller.Velocity = targetVelocity;

        // --- transitions ---

        // dash
        if ( Input.Pressed( "run" ) && !Manager.HasAirDashed && !Input.AnalogMove.IsNearlyZero() && Manager.Stamina.TryConsume( 1f ) )
        {
            return new DashState( Manager );
        }

        // landed
        if ( Manager.Controller.IsOnGround )
        {
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

            // slide queuing (if they hold crouch while falling)
            // ommiting the z axis so fall speed doesn't factor into slide threshold check
            if ( Input.Down( "crouch" ) && Manager.Controller.Velocity.WithZ(0).Length > currentSlideThreshold )
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
