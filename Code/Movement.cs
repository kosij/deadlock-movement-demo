using Sandbox;

public sealed class Movement : Component
{
    // These show up in your Inspector so you can tweak them without code
    [Property] public float Speed { get; set; } = 300f;
    [Property] public float Gravity { get; set; } = 800f;
    [Property] public float JumpForce { get; set; } = 300f;

    // This is the "Physics" part we talked about
    [RequireComponent] public CharacterController Controller { get; set; }

    protected override void OnUpdate()
    {
        // input direction
        Vector3 wishDir = Input.AnalogMove;

        // make direction relative to camera
        wishDir *= Scene.Camera.WorldRotation;

        wishDir.z = 0;

        Vector3 targetVelocity = wishDir * Speed;

        targetVelocity.z = Controller.Velocity.z;

        if ( Controller.IsOnGround )
        {
            targetVelocity.z = 0;

        }
        else
        {
            targetVelocity.z -= Gravity * Time.Delta;
        }

        Controller.Velocity = targetVelocity;

        if ( Controller.IsOnGround && Input.Pressed( "jump" ) )
        {
            Controller.Punch( Vector3.Up * JumpForce );
        }

        Controller.Move();
    }
}