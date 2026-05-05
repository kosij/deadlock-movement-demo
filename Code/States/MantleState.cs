using Sandbox;
using System;

// fixed-duration position lerp onto or over an obstacle.
// transitions: complete -> GroundedState | complete + crouch held -> SlideState.
public class MantleState : BaseState
{
    private Vector3 StartPos;
    private Vector3 TargetPos;
    private TimeSince TimeSinceEntered;

    public MantleState( Movement manager, Vector3 targetPos ) : base( manager )
    {
        TargetPos = targetPos;
    }

    public override void Enter()
    {
        StartPos = Manager.Transform.Position;
        TimeSinceEntered = 0f;

        // zero velocity so Controller.Move() doesn't fight the position lerp
        Manager.Controller.Velocity = Vector3.Zero;
    }

    public override BaseState Update()
    {
        float t = Math.Clamp( (float)TimeSinceEntered / Manager.MantleDuration, 0f, 1f );

        // sine-based arced interpolation to avoid clipping the wall corner
        float zT = MathF.Sin( t * MathF.PI / 2f );
        float xyT = 1f - MathF.Cos( t * MathF.PI / 2f );

        Vector3 currentPos = StartPos;
        currentPos.z = StartPos.z.LerpTo( TargetPos.z, zT );
        currentPos.x = StartPos.x.LerpTo( TargetPos.x, xyT );
        currentPos.y = StartPos.y.LerpTo( TargetPos.y, xyT );

        Manager.WorldPosition = currentPos;

        // keep velocity zero each frame so Controller.Move() is a no-op during the lerp
        Manager.Controller.Velocity = Vector3.Zero;

        // face the obstacle during the climb
        WishDir = ( TargetPos - StartPos ).WithZ( 0 ).Normal;

        if ( t >= 1f )
        {
            // get local wish direction
            Vector3 wishDir = GetWishDir();

            Vector3 mantleForward = ( TargetPos - StartPos ).WithZ( 0 ).Normal;

            // transition to slide if holding crouch + wishDir towards wall
            if ( Input.Down( "crouch" ) && Vector3.Dot( wishDir, mantleForward ) > 0.5f )
            {
                Manager.Controller.Velocity = mantleForward * Manager.MantleSlideImpulse;
                return new SlideState( Manager );
            }

            // standard mantle exit
            Manager.Controller.Velocity = mantleForward * Manager.MantleExitImpulse;
            return new GroundedState( Manager );
        }

        return null;
    }
}
