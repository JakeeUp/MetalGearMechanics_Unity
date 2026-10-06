// Base class for a guard behavior. AIController owns one instance of each
// state and switches between them with ChangeState.
public abstract class AIState
{
    protected readonly AIController ai;

    protected AIState(AIController ai)
    {
        this.ai = ai;
    }

    // True while the guard knows about the player. The alarm timer only
    // counts down in alerted states, and other guards treat an alerted guard
    // as already hunting.
    public virtual bool IsAlerted => false;

    public virtual void Enter(AIState previous) { }
    public abstract void Tick(float delta);
    public virtual void Exit() { }
}
