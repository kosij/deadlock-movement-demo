using Sandbox;

public abstract class BaseState
{
    protected Movement Manager;

    public BaseState( Movement manager )
    {
        Manager = manager;
    }

    public Vector3 WishDir { get; protected set; }

    public virtual void Enter() { }

    public abstract BaseState Update();
    
    public virtual void Exit() { }
}
