using Sandbox;
using System;

public sealed class Movement : Component
{
    [Property] public float Speed { get; set; } = 300f;
    [Property] public float Gravity { get; set; } = 800f;
    [Property] public float JumpForce { get; set; } = 300f;
    [Property] public float GroundFriction { get; set; } = 4f;
    [Property] public float GroundAcceleration { get; set; } = 10f;
    [Property] public float AirSpeed { get; set; } = 30f;
    [Property] public float AirAcceleration { get; set; } = 100f;

    [RequireComponent] public CharacterController Controller { get; set; }

    protected override void OnUpdate()
    {
        // input direction
        Vector3 wishDir = Input.AnalogMove;
        
        // make direction relative to camera
        wishDir *= Scene.Camera.WorldRotation;

        wishDir.z = 0;

        // inherit momentum from last frame
        Vector3 targetVelocity = Controller.Velocity;

        if ( Controller.IsOnGround )
        {
            // apply friciton
            targetVelocity = Vector3.Lerp( targetVelocity, Vector3.Zero, GroundFriction * Time.Delta );

            // calculate speed dot product and budget
            float currentSpeed = Vector3.Dot( targetVelocity, wishDir );
            float speedBudget = Speed - currentSpeed;

            if ( speedBudget > 0 )
            {
                // calculate accel and clamp it
                float accelAmount = Math.Min( GroundAcceleration * Speed * Time.Delta, speedBudget );
                targetVelocity += wishDir * accelAmount;
            }

            // zero out gravity while grounded
            targetVelocity.z = 0;
        }
        else
        {
            // calculate speed dot product and budget
            float currentSpeed = Vector3.Dot( targetVelocity, wishDir );
            float speedBudget = Speed - currentSpeed;

            if ( speedBudget > 0 )
            {
                float accelAmount = Math.Min( AirAcceleration * AirSpeed * Time.Delta, speedBudget );
                targetVelocity += wishDir * accelAmount;
            }

            // add gravity
            targetVelocity.z -= Gravity * Time.Delta;
        }

        Controller.Velocity = targetVelocity;

        // rotate player with camera
        Transform.Rotation = Rotation.FromYaw( Scene.Camera.WorldRotation.Angles().yaw );

        // jump
        if ( Controller.IsOnGround && Input.Pressed( "jump" ) )
        {
            Controller.Punch( Vector3.Up * JumpForce );
        }

        Controller.Move();
    }
}