using UnityEngine;

// Chases the player while they're in sight and fires in bursts once in range,
// pausing in CautionState between bursts. Losing sight of the player hands
// over to SearchState.
public class CombatState : AIState
{
    int bulletsToFire;
    int timesShot;
    float currentFire;
    bool initRange;

    public CombatState(AIController ai) : base(ai) { }

    public override bool IsAlerted => true;

    public override void Enter(AIState previous)
    {
        ai.agent.speed = ai.aggressiveSpeed;

        // Coming back from a pause between bursts keeps the current burst going.
        if (!(previous is CautionState))
            initRange = false;
    }

    public override void Tick(float delta)
    {
        // Seeing the player also refreshes the last known position and alarm.
        if (!ai.CanSeeTarget())
        {
            ai.LoseTarget();
            ai.ChangeState(ai.searchState);
            return;
        }

        ai.agent.SetDestination(ai.lastKnownPosition);

        float sqrDis = (ai.lastKnownPosition - ai.mTransform.position).sqrMagnitude;
        bool inRange = sqrDis < ai.SqrAttackDistance;

        if (inRange)
        {
            HandleFiring(delta);
        }
        else
        {
            initRange = false;
            ai.agent.updateRotation = true;
            ai.agent.isStopped = false;
            ai.HandleDetection();
        }

        ai.animator.SetFloat(AIController.hashMovement, inRange ? 0 : 1, 0.1f, delta);
    }

    void HandleFiring(float delta)
    {
        if (!initRange)
        {
            StartBurst(delta);
            currentFire = ai.fireRate;
            initRange = true;
        }

        ai.agent.isStopped = true;
        ai.LookAtLastKnownPosition(delta);

        if (currentFire >= 0)
        {
            currentFire -= delta;
            return;
        }

        currentFire = ai.fireRate;
        Shoot();

        if (bulletsToFire <= 0)
            StartBurst(delta);
    }

    void StartBurst(float delta)
    {
        bulletsToFire = Mathf.Min(Random.Range(5, 20), ai.magBullets - timesShot);
        ai.EnterCaution(ai.cautionTimerNormal, delta, false);
    }

    void Shoot()
    {
        timesShot++;
        bulletsToFire--;
        ai.FireWeapon();

        if (timesShot > ai.magBullets)
        {
            timesShot = 0;
            ai.PlayReload();
        }
    }
}
