using Sandbox;

public sealed class Movement : Component
{
    // These show up in your Inspector so you can tweak them without code
    [Property] public float Speed { get; set; } = 300f;

    // This is the "Physics" part we talked about
    [RequireComponent] public CharacterController Controller { get; set; }

    protected override void OnUpdate()
    {
        // input direction
        Vector3 wishDir = Input.AnalogMove;

        // make direction relative to camera
        wishDir *= Scene.Camera.WorldRotation;
        
        // remove z direction
        wishDir.z = 0;

        // tell character controller where to go
        Controller.Velocity = wishDir * Speed;

        Controller.Move();
    }
}