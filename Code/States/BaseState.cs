using Sandbox;

// base class for all movement states.
// each state owns its own velocity logic and returns the next state from Update().
// returning null = stay in this state. returning a new state = transition.
// WishDir is written each frame by the active state and read by the animation system in Movement.cs.
// to add a new state: extend BaseState, implement Update(), add transition logic in the relevant state(s).
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
