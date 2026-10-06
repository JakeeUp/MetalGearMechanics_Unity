// Held by the player in CQC. The player's grab code moves and animates the
// guard, so this state does nothing each frame. An alert raised during the
// grab is remembered so the guard counts as alerted once it breaks free.
public class GrabbedState : AIState
{
    bool alerted;

    public GrabbedState(AIController ai) : base(ai) { }

    public override bool IsAlerted => alerted;

    public void MarkAlerted()
    {
        alerted = true;
    }

    public override void Enter(AIState previous)
    {
        alerted = previous != null && previous.IsAlerted;
    }

    public override void Tick(float delta) { }
}
