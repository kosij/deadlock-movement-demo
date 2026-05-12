using Sandbox;
using System;

public sealed class PlayerCamera : Component
{
    [Property] public GameObject Target { get; set; }
    [Property] public float Distance { get; set; } = 250f;
    [Property] public float TargetHeight { get; set; } = 1.05f; // 1.0 = head height
    [Property] public float ShoulderOffset { get; set; } = 40f;  // lateral right-side offset
    [Property] public float FollowSpeed { get; set; } = 20f;     // tight follow (everything except ground dash)
    [Property] public float DashFollowSpeed { get; set; } = 5f;  // lagged follow during ground dash

    private Angles EyeAngles;
    private Vector3 SmoothedPivot;  // lerps toward character position - rotation offset applied on top
    private bool Initialized;
    private Movement MovementRef;

    protected override void OnUpdate()
    {
        if ( Target == null ) return;

        // get mouse input
        EyeAngles += Input.AnalogLook;

        // stop camera from turning upside down
        EyeAngles.pitch = EyeAngles.pitch.Clamp( -89f, 89f );

        // apply rotation to scene camera
        Scene.Camera.WorldRotation = EyeAngles.ToRotation();

    // --- position and follow ---

        // set the camera offset from the player
        MovementRef ??= Target.Components.Get<Movement>();
        CharacterController controller = Target.Components.Get<CharacterController>();
        float currentTargetHeight = controller != null ? (controller.Height * TargetHeight) : 64f;
        Vector3 centerPosition = Target.WorldPosition + Vector3.Up * currentTargetHeight;

        // detect ground dash for slower follow speed
        // lerp the PIVOT (character follow), not the final camera position.
        // this means mouse rotation is always instant - only character movement lags.
        bool isGroundDash = MovementRef?.CurrentState is DashState && MovementRef?.Controller.IsOnGround == true;
        float speed = isGroundDash ? DashFollowSpeed : FollowSpeed;

        if ( !Initialized ) { SmoothedPivot = centerPosition; Initialized = true; }
        SmoothedPivot = Vector3.Lerp( SmoothedPivot, centerPosition, speed * Time.Delta );

        // apply rotational offset instantly from the smoothed pivot
        Vector3 offset = Scene.Camera.WorldRotation.Backward * Distance
                       + Scene.Camera.WorldRotation.Right * ShoulderOffset;

        var tr = Scene.Trace.Ray( SmoothedPivot, SmoothedPivot + offset )
            .IgnoreGameObjectHierarchy( Target )
            .Radius( 8f )
            .Run();

        Scene.Camera.WorldPosition = tr.Hit ? tr.EndPosition : SmoothedPivot + offset;
    }
}
