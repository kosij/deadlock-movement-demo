using Sandbox;
using System;

public sealed class PlayerCamera : Component
{
    [Property] public GameObject Target { get; set; }
    [Property] public float Distance { get; set; } = 250f;
    [Property] public float TargetHeight { get; set; } = 64f;

    private Angles EyeAngles;

    protected override void OnUpdate()
    {
        if ( Target == null ) return;

        // get mouse input
        EyeAngles += Input.AnalogLook;

        // stop camera from turning upside down
        EyeAngles.pitch = EyeAngles.pitch.Clamp( -89f, 89f );

        // apply rotation to scene camera
        Scene.Camera.WorldRotation = EyeAngles.ToRotation();

        // set the camera offset from the player
        CharacterController controller = Target.Components.Get<CharacterController>();
        float currentTargetHeight = controller != null ? (controller.Height * 0.9f) : 64f;
        Vector3 centerPosition = Target.WorldPosition + Vector3.Up * currentTargetHeight;
        Vector3 offset = Scene.Camera.WorldRotation.Backward * Distance;

        // trace to prevent camera clipping through walls
        var tr = Scene.Trace.Ray( centerPosition, centerPosition + offset )
            .IgnoreGameObjectHierarchy( Target )
            .Radius( 8f ) // small sphere so the camera doesn't touch the wall and clip
            .Run();

        // apply position to scene camera (move it inward if it hit a wall)
        Scene.Camera.WorldPosition = tr.Hit ? tr.EndPosition : centerPosition + offset;
    }
}
