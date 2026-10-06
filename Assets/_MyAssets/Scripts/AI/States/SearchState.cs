using UnityEngine;
using UnityEngine.AI;

// The player broke line of sight. The guard runs to the last known position,
// looks the way the player was heading, then alternates between scanning in
// random directions and checking random nearby spots until it spots the
// player again or the alarm runs out.
public class SearchState : AIState
{
    enum Phase { Scan, Wander }

    Phase phase;
    float scanTime;
    bool hasTargetRotation;

    public SearchState(AIController ai) : base(ai) { }

    public override bool IsAlerted => true;

    public override void Enter(AIState previous)
    {
        ai.agent.speed = ai.aggressiveSpeed;

        // Fresh off a chase, so first look where the player was last heading.
        if (previous is CombatState)
            StartScan(Phase.Scan);
    }

    public override void Tick(float delta)
    {
        ai.agent.SetDestination(ai.lastKnownPosition);
        ai.agent.updateRotation = true;
        ai.agent.isStopped = false;

        ai.HandleDetection();
        if (ai.HasTarget)
        {
            ai.ChangeState(ai.combatState);
            return;
        }

        if (ReachedDestination())
        {
            if (hasTargetRotation)
                Scan(delta);
            else
                PickNextLook();
        }

        float movement = ai.agent.desiredVelocity.sqrMagnitude > 0.01f ? 1 : 0;
        ai.animator.SetFloat(AIController.hashMovement, movement, 0.1f, delta);
    }

    bool ReachedDestination()
    {
        NavMeshAgent agent = ai.agent;
        return agent.remainingDistance < agent.stoppingDistance
            || agent.pathStatus == NavMeshPathStatus.PathInvalid
            || agent.pathStatus == NavMeshPathStatus.PathPartial;
    }

    void Scan(float delta)
    {
        ai.RotateTowards(ai.lastKnownDirection, delta);

        scanTime -= delta;
        if (scanTime < 0)
        {
            hasTargetRotation = false;
            phase = Random.Range(0, 100) > 50 ? Phase.Wander : Phase.Scan;
        }
    }

    void PickNextLook()
    {
        if (phase == Phase.Wander)
            WalkToRandomSpot();

        Vector2 r = Random.insideUnitCircle;
        ai.lastKnownDirection = new Vector3(r.x, 0, r.y);
        StartScan(phase);
    }

    void StartScan(Phase nextPhase)
    {
        phase = nextPhase;
        scanTime = Random.Range(ai.minScanTime, ai.maxScanTime);
        hasTargetRotation = true;
    }

    void WalkToRandomSpot()
    {
        Vector3 r = Random.insideUnitSphere * ai.fovRadius;
        if (NavMesh.SamplePosition(ai.mTransform.position + r, out NavMeshHit hit, 5, NavMesh.AllAreas))
            ai.lastKnownPosition = hit.position;
    }
}
