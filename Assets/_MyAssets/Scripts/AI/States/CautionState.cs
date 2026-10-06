// The short "?!" reaction. The guard stops and turns toward the last known
// position, then chases if it still has a target or searches if it doesn't.
// Combat also drops into this state between bursts of fire.
public class CautionState : AIState
{
    float timer;

    public CautionState(AIController ai) : base(ai) { }

    public override bool IsAlerted => true;

    public void SetDuration(float duration)
    {
        timer = duration;
    }

    public override void Tick(float delta)
    {
        if (timer < 0)
        {
            ai.agent.isStopped = false;
            ai.ChangeState(ai.HasTarget ? (AIState)ai.combatState : ai.searchState);
            return;
        }

        if (ai.animator.GetBool(AIController.hashCanRotate))
            ai.LookAtLastKnownPosition(delta);

        ai.agent.isStopped = true;
        timer -= delta;
    }
}
