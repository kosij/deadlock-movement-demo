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
    [Property] public float SlideFriction { get; set; } = 0.5f;
    [Property] public float MinSlideSpeed { get; set; } = 150f;
    [Property] public float CrouchSpeed { get; set; } = 100f;


    [RequireComponent] public CharacterController Controller { get; set; }
    public BaseState CurrentState { get; private set; }

    protected override void OnStart()
    {
        CurrentState = new AirborneState( this );
        CurrentState.Enter();
    }

    protected override void OnUpdate()
    {
        BaseState nextState = CurrentState.Update();

        if ( nextState != null )
        {
            CurrentState.Exit();
            CurrentState = nextState;
            CurrentState.Enter();
        }

        // rotate player with camera
        Transform.Rotation = Rotation.FromYaw( Scene.Camera.WorldRotation.Angles().yaw );

        Controller.Move();
    }
}