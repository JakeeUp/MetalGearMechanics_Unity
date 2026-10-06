using UnityEngine;

// Walks the waypoint route, waits and looks around at each point, and checks
// the view cone for the player every frame.
public class PatrolState : AIState
{
    float waitTimer;

    public PatrolState(AIController ai) : base(ai) { }

    public override void Enter(AIState previous)
    {
        ai.agent.speed = ai.normalSpeed;
        ai.agent.isStopped = false;
    }

    public override void Tick(float delta)
    {
        ai.HandleDetection();
        if (ai.CurrentState != this)
            return;

        if (ai.waypoints.Length == 0)
            return;

        Waypoint waypoint = ai.waypoints[ai.index];
        Vector3 waypointPos = waypoint.targetPosition.position;
        float sqrDis = (ai.mTransform.position - waypointPos).sqrMagnitude;
        float sqrStopDis = ai.agent.stoppingDistance * ai.agent.stoppingDistance;

        if (sqrDis > sqrStopDis)
        {
            ai.animator.SetFloat(AIController.hashMovement, 1, 0.1f, delta);
            ai.agent.updateRotation = true;

            if (!ai.agent.hasPath)
                ai.agent.SetDestination(waypointPos);
            return;
        }

        ai.animator.SetFloat(AIController.hashMovement, 0, 0.1f, delta);
        ai.agent.updateRotation = false;

        Quaternion targetRot = Quaternion.Euler(waypoint.lookEulers);
        ai.mTransform.rotation = Quaternion.Slerp(ai.mTransform.rotation, targetRot, delta / ai.rotateSpeed);

        waitTimer += delta;
        if (waitTimer >= waypoint.waitTime)
        {
            waitTimer = 0;
            ai.index = (ai.index + 1) % ai.waypoints.Length;
        }
    }
}
