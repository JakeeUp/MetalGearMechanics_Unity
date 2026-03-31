using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class AIController : MonoBehaviour, IShootable, IPointOfInterest
{
    // ============================
    // Component References
    // ============================

    NavMeshAgent agent;
    new Rigidbody rigidbody;
    public Animator animator;
    InventoryManager inventoryManager;
    Transform mTransform;

    // ============================
    // Health
    // ============================

    public float currentHealth { get; private set; }
    public float maxHealth = 100f;

    // ============================
    // Waypoints
    // ============================

    [Header("Waypoint Index")]
    [Space(5)]
    public int index;
    public Waypoint[] waypoints;
    Waypoint currentWaypoint;

    // ============================
    // State Flags
    // ============================

    [Header("Bools")]
    [Space(5)]
    [SerializeField] private bool _isAgressive;
    public bool isAgressive { get { return _isAgressive; } set { _isAgressive = value; } }
    [SerializeField] private bool isCaution;
    [SerializeField] private bool isGrab;
    public bool isDead;
    public bool isSpottedDead;

    // ============================
    // Timers
    // ============================

    [Header("Wait Timer")]
    [Space(5)]
    float waitTimer;
    float cautionTimer;
    public float alarmTimer;
    public float cautionTimerNormal = .7f;

    // ============================
    // Movement
    // ============================

    [Header("Attributes")]
    [Space(5)]
    public float normalSpeed = 2;
    public float aggressiveSpeed = 4;
    public float rotateSpeed = .5f;
    public float fovRadius = 20;
    public float fovAngle = 45;

    // ============================
    // Combat
    // ============================

    [Header("Attack Attributes")]
    [Space(5)]
    [SerializeField] private float damageAmount = 10f;
    public float weaponSpread = .3f;
    public int magBullets = 40;
    int bulletsToFire;
    int timesShot;
    public int timesStruggle;
    float lastCautionPlayed;

    public float DamageAmount
    {
        get { return damageAmount; }
        set { damageAmount = value; }
    }

    public float attackDistance = 5;
    Vector3 lastKnownPosition;
    Vector3 lastKnownDirection;
    Controller currentTarget;
    LayerMask controllerLayer;
    LayerMask ignoreForDetection;

    // ============================
    // UI
    // ============================

    public TextMeshPro emotionText;
    public GameObject emotionObj;

    // ============================
    // Audio
    // ============================

    [Header("Sound Attributes")]
    [Space(5)]
    [SerializeField] private AudioSource hitSoundSource;
    [SerializeField] private AudioClip[] hitSoundClips;

    // ============================
    // Shooting State
    // ============================

    public float fireRate = .1f;
    float currentFire;
    bool initRange;
    public ParticleSystem muzzleFire;

    // ============================
    // Search / Scan
    // ============================

    public enum AIPhase { scanRan, searchRan, searchPOI }

    [Header("Scan Settings")]
    [Space(5)]
    public AIPhase aIPhase;
    public float scanTime;
    public float minScanTime = 1;
    public float maxScanTime = 3;
    public bool hasTargetRotation;

    // ============================
    // Detection
    // ============================

    public Transform poiTransform;
    public bool spotted;
    public string hitFx = "blood";

    // ============================
    // Physics Buffers (NonAlloc)
    // ============================

    static readonly Collider[] detectionBuffer = new Collider[16];

    // ============================
    // Cached Values
    // ============================

    float cachedFovAngleCos;
    float sqrAttackDistance;

    // ============================
    // Lifecycle
    // ============================

    private void Start()
    {
        hitSoundSource = GetComponent<AudioSource>();
        agent = GetComponentInChildren<NavMeshAgent>();
        rigidbody = GetComponentInChildren<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        inventoryManager = GetComponentInChildren<InventoryManager>();
        mTransform = transform;

        if (waypoints.Length > 0)
            currentWaypoint = waypoints[index];

        animator.applyRootMotion = false;
        controllerLayer = 1 << 9;
        ignoreForDetection = ~(1 << 12 | 1 << 13);
        currentHealth = maxHealth;
        GameReferences.damage = damageAmount;

        cachedFovAngleCos = Mathf.Cos(fovAngle * Mathf.Deg2Rad);
        sqrAttackDistance = attackDistance * attackDistance;
    }

    private void Update()
    {
        float delta = Time.deltaTime;

        if (currentHealth <= 0)
        {
            animator.Play("grab_death");
            isDead = true;
            enabled = false;
            return;
        }

        if (isGrab)
            return;

        if (animator.GetBool("isInteracting"))
        {
            agent.isStopped = true;
            if (animator.GetBool("canRotate"))
                HandleLookAtTarget(delta);
            return;
        }

        animator.SetBool("isAggressive", isAgressive);
        animator.SetBool("isCaution", isCaution);

        if (!isAgressive)
        {
            agent.speed = normalSpeed;
            HandleDetection();
            HandleNormalLogic(delta);
        }
        else
        {
            HandleAggressiveState(delta);
        }
    }

    // ============================
    // Normal Patrol Logic
    // ============================

    private void HandleNormalLogic(float delta)
    {
        if (waypoints.Length == 0)
            return;

        currentWaypoint = waypoints[index];
        Vector3 waypointPos = currentWaypoint.targetPosition.position;
        float sqrDis = (mTransform.position - waypointPos).sqrMagnitude;
        float sqrStopDis = agent.stoppingDistance * agent.stoppingDistance;

        if (sqrDis > sqrStopDis)
        {
            animator.SetFloat("movement", 1, 0.1f, delta);
            agent.updateRotation = true;

            if (!agent.hasPath)
                agent.SetDestination(waypointPos);
        }
        else
        {
            animator.SetFloat("movement", 0, 0.1f, delta);
            agent.updateRotation = false;

            Quaternion targetRot = Quaternion.Euler(currentWaypoint.lookEulers);
            mTransform.rotation = Quaternion.Slerp(mTransform.rotation, targetRot, delta / rotateSpeed);

            waitTimer += delta;
            if (waitTimer >= currentWaypoint.waitTime)
            {
                waitTimer = 0;
                index = (index + 1) % waypoints.Length;
            }
        }
    }

    // ============================
    // Aggressive State
    // ============================

    private void HandleAggressiveState(float delta)
    {
        if (isCaution)
        {
            if (cautionTimer < 0)
            {
                isCaution = false;
                agent.isStopped = false;
            }
            else
            {
                if (animator.GetBool("canRotate"))
                    HandleLookAtTarget(delta);

                agent.isStopped = true;
                cautionTimer -= delta;
            }
        }
        else
        {
            agent.speed = aggressiveSpeed;
            HandleAggressiveLogic(delta);
        }

        if (alarmTimer > 0)
        {
            alarmTimer -= delta;
        }
        else
        {
            alarmTimer = 0;
            isCaution = false;
            isAgressive = false;
            currentTarget = null;
        }
    }

    private void HandleAggressiveLogic(float delta)
    {
        if (currentTarget != null && !RaycastToTarget(currentTarget))
        {
            lastKnownDirection = (currentTarget.mTransform.position - lastKnownPosition).normalized;
            hasTargetRotation = true;
            scanTime = Random.Range(minScanTime, maxScanTime);
            aIPhase = AIPhase.scanRan;
            currentTarget = null;
        }

        bool inRange = false;
        float sqrDis = (lastKnownPosition - mTransform.position).sqrMagnitude;
        agent.SetDestination(lastKnownPosition);

        if (currentTarget != null)
        {
            if (sqrDis < sqrAttackDistance)
            {
                inRange = true;
                HandleInRangeCombat(delta);
            }
            else
            {
                initRange = false;
                agent.updateRotation = true;
                agent.isStopped = false;
                HandleDetection();
            }
        }
        else
        {
            initRange = false;
            agent.updateRotation = true;
            agent.isStopped = false;
            HandleDetection();
            HandleSearchBehavior(delta);
        }

        HandleAggressiveAnimations(inRange, delta);
    }

    private void HandleInRangeCombat(float delta)
    {
        if (!initRange)
        {
            AssignRandomBulletsToFire();
            PlayCautionState(cautionTimerNormal, delta, false);
            currentFire = fireRate;
            initRange = true;
        }

        agent.isStopped = true;
        HandleLookAtTarget(delta);

        if (currentFire < 0)
        {
            currentFire = fireRate;
            HandleShooting();

            if (bulletsToFire <= 0)
            {
                AssignRandomBulletsToFire();
                PlayCautionState(cautionTimerNormal, delta, false);
            }
        }
        else
        {
            currentFire -= delta;
        }
    }

    private void HandleSearchBehavior(float delta)
    {
        bool atDestination = agent.remainingDistance < agent.stoppingDistance
            || agent.pathStatus == NavMeshPathStatus.PathInvalid
            || agent.pathStatus == NavMeshPathStatus.PathPartial;

        if (!atDestination)
            return;

        if (hasTargetRotation)
        {
            aIPhase = AIPhase.scanRan;
            HandleRotation(lastKnownDirection, delta);

            scanTime -= delta;
            if (scanTime < 0)
            {
                hasTargetRotation = false;
                if (Random.Range(0, 100) > 50)
                    aIPhase = AIPhase.searchRan;
            }
        }
        else
        {
            switch (aIPhase)
            {
                case AIPhase.scanRan:
                    FindRandomLookDirection();
                    break;
                case AIPhase.searchRan:
                    SearchRandomPosition();
                    FindRandomLookDirection();
                    break;
                case AIPhase.searchPOI:
                    break;
            }
        }
    }

    private void HandleAggressiveAnimations(bool inRange, float delta)
    {
        if (currentTarget != null)
        {
            animator.SetFloat("movement", inRange ? 0 : 1, 0.1f, delta);
        }
        else
        {
            float movement = agent.desiredVelocity.sqrMagnitude > 0.01f ? 1 : 0;
            animator.SetFloat("movement", movement, 0.1f, delta);
        }
    }

    // ============================
    // Search Helpers
    // ============================

    private void FindRandomLookDirection()
    {
        Vector2 r = Random.insideUnitCircle;
        lastKnownDirection.x = r.x;
        lastKnownDirection.z = r.y;
        scanTime = Random.Range(minScanTime, maxScanTime);
        hasTargetRotation = true;
    }

    private void SearchRandomPosition()
    {
        Vector3 r = Random.insideUnitSphere * fovRadius;
        if (NavMesh.SamplePosition(mTransform.position + r, out NavMeshHit hit, 5, NavMesh.AllAreas))
            lastKnownPosition = hit.position;
    }

    // ============================
    // Combat
    // ============================

    private void AssignRandomBulletsToFire()
    {
        bulletsToFire = Random.Range(5, 20);
        int remaining = magBullets - timesShot;
        if (bulletsToFire > remaining)
            bulletsToFire = remaining;
    }

    private void HandleShooting()
    {
        timesShot++;
        bulletsToFire--;

        if (inventoryManager != null && inventoryManager.currentWeaponHook != null)
        {
            GameReferences.RaycastShoot(mTransform, inventoryManager.currentWeaponHook);
            inventoryManager.currentWeaponHook.Shoot();
        }

        if (timesShot > magBullets)
        {
            timesShot = 0;
            animator.CrossFade("Reload", 0.2f);
            animator.CrossFade("Reload_Body", 0.2f);
        }
    }

    // ============================
    // Rotation
    // ============================

    private void HandleLookAtTarget(float delta)
    {
        Vector3 dir = lastKnownPosition - mTransform.position;
        HandleRotation(dir, delta);
    }

    private void HandleRotation(Vector3 dir, float delta)
    {
        dir.y = 0;
        if (dir == Vector3.zero)
            dir = mTransform.forward;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        mTransform.rotation = Quaternion.Slerp(mTransform.rotation, targetRot, delta / rotateSpeed);
        agent.updateRotation = false;
    }

    // ============================
    // Caution State
    // ============================

    private void PlayCautionState(float timer, float delta, bool crossfadeToState = true)
    {
        isCaution = true;
        cautionTimer = timer;

        if (!isGrab && crossfadeToState)
            animator.CrossFade("caution", 0.2f);

        animator.SetFloat("movement", 0, 0.1f, delta);
    }

    // ============================
    // Detection (NonAlloc)
    // ============================

    bool RaycastToTarget(IPointOfInterest poi)
    {
        Vector3 poiPos = poi.GetTransform().position;
        Vector3 dir = poiPos - mTransform.position;
        dir.Normalize();

        // Fast angle check using dot product instead of Vector3.Angle
        float dot = Vector3.Dot(mTransform.forward, dir);
        if (dot < cachedFovAngleCos)
            return false;

        Vector3 origin = mTransform.position;
        origin.y += 1;

        if (!Physics.Raycast(origin, dir, out RaycastHit hit, 100, ignoreForDetection))
            return false;

        IPointOfInterest pointOfInterest = hit.transform.GetComponentInParent<IPointOfInterest>();
        if (pointOfInterest == null)
            return false;

        spotted = true;
        return pointOfInterest.OnDetect(this);
    }

    private void HandleDetection()
    {
        int count = Physics.OverlapSphereNonAlloc(mTransform.position, fovRadius, detectionBuffer, controllerLayer);

        for (int i = 0; i < count; i++)
        {
            IPointOfInterest poi = detectionBuffer[i].transform.GetComponentInParent<IPointOfInterest>();
            if (poi != null && poi.GetTransform() != poiTransform)
            {
                if (RaycastToTarget(poi))
                    break;
            }
        }
    }

    // ============================
    // Public API — Player Detection
    // ============================

    public void OnDetectPlayer(Controller targetPlayer)
    {
        alarmTimer = 25;
        currentTarget = targetPlayer;
        lastKnownPosition = currentTarget.transform.position;
        SetToCautiousState();
    }

    public void SetToCautiousState(bool force = false)
    {
        if (!isAgressive || force)
        {
            emotionText.text = "?!";
            emotionObj.SetActive(true);

            cautionTimer = cautionTimerNormal;
            isCaution = true;
            isAgressive = true;
            alarmTimer = 25;

            if (!isGrab)
                animator.CrossFade("caution", 0.2f);

            GameReferences.UpdateLastKnownPositionOfCloseby(lastKnownPosition, 15);
        }
    }

    public void UpdateLastKnowPosition(Vector3 newPosition)
    {
        if (currentTarget != null)
            return;

        lastKnownPosition = newPosition;

        if (!isAgressive || Time.realtimeSinceStartup - lastCautionPlayed > 4)
        {
            lastCautionPlayed = Time.realtimeSinceStartup;
            SetToCautiousState();
        }
    }

    // ============================
    // Public API — Grab System
    // ============================

    public void StartGrab(Vector3 tp, Quaternion targetRotation)
    {
        agent.enabled = false;
        mTransform.position = tp;
        isGrab = true;
        animator.Play("e_grab_start");
        mTransform.rotation = targetRotation;

        emotionText.text = "?!";
        emotionObj.SetActive(true);

        GameReferences.UpdateLastKnownPositionOfCloseby(mTransform.position, 2);
    }

    public void KillByGrab()
    {
        animator.Play("grab_death");
        isDead = true;
        enabled = false;
    }

    public void StopGrab(Controller target)
    {
        currentTarget = target;
        lastKnownPosition = currentTarget.mTransform.position;
        agent.enabled = true;
        agent.updateRotation = true;
        isGrab = false;
        animator.Play("e_grab_cancel");
        PlayCautionState(cautionTimerNormal, Time.deltaTime, false);
    }

    // ============================
    // IShootable Implementation
    // ============================

    public void OnHit()
    {
    }

    public string GetHitFx()
    {
        return hitFx;
    }

    public void OnHit(float dmgAmt)
    {
        currentHealth -= dmgAmt;
        currentHealth = Mathf.Max(currentHealth, 0);

        if (hitSoundSource != null && hitSoundClips != null && hitSoundClips.Length > 0)
        {
            int clipIndex = Random.Range(0, hitSoundClips.Length);
            hitSoundSource.clip = hitSoundClips[clipIndex];
            hitSoundSource.Play();
        }

        UpdateLastKnowPosition(transform.position);
    }

    // ============================
    // IPointOfInterest Implementation
    // ============================

    public bool OnDetect(AIController aIController)
    {
        if (!isDead)
            return false;

        if (!isSpottedDead)
        {
            aIController.emotionText.text = "?";
            aIController.emotionObj.SetActive(true);
            aIController.UpdateLastKnowPosition(mTransform.position);
            isSpottedDead = true;
        }

        return true;
    }

    public Transform GetTransform()
    {
        return poiTransform;
    }
}

[System.Serializable]
public class Waypoint
{
    public Transform targetPosition;
    public Vector3 lookEulers;
    public float waitTime;
}
