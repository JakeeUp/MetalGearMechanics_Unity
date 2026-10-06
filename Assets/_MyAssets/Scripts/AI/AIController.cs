using TMPro;
using UnityEngine;
using UnityEngine.AI;

// Guard AI. Behavior lives in the state classes under AI/States; this class
// owns the components and tuning values, the shared helpers the states use
// (detection, rotation, shooting), and the public API other systems call.
public class AIController : MonoBehaviour, IShootable, IPointOfInterest
{
    // ============================
    // Component References
    // ============================

    internal NavMeshAgent agent;
    public Animator animator;
    InventoryManager inventoryManager;
    internal Transform mTransform;

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

    // ============================
    // State Machine
    // ============================

    [Header("State")]
    [Space(5)]
    [SerializeField, Tooltip("Read only. Shows the active state for debugging.")]
    private string currentStateName;
    public bool isDead;
    public bool isSpottedDead;

    internal PatrolState patrolState;
    internal CautionState cautionState;
    internal CombatState combatState;
    internal SearchState searchState;
    internal GrabbedState grabbedState;

    public AIState CurrentState { get; private set; }

    // True from the moment the guard is alerted until the alarm runs out.
    public bool isAgressive => CurrentState != null && CurrentState.IsAlerted;

    // ============================
    // Timers
    // ============================

    [Header("Wait Timer")]
    [Space(5)]
    public float alarmTimer;
    public float cautionTimerNormal = .7f;
    const float AlarmDuration = 25f;

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
    public int timesStruggle;
    float lastCautionPlayed;

    public float DamageAmount
    {
        get { return damageAmount; }
        set { damageAmount = value; }
    }

    public float attackDistance = 5;
    internal Vector3 lastKnownPosition;
    internal Vector3 lastKnownDirection;
    Controller currentTarget;
    LayerMask controllerLayer;
    LayerMask ignoreForDetection;

    public bool HasTarget => currentTarget != null;
    public float SqrAttackDistance => sqrAttackDistance;

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
    // Shooting
    // ============================

    public float fireRate = .1f;
    public ParticleSystem muzzleFire;

    // ============================
    // Search / Scan
    // ============================

    [Header("Scan Settings")]
    [Space(5)]
    public float minScanTime = 1;
    public float maxScanTime = 3;

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
    // Animator Hashes
    // ============================

    static readonly int hashIsInteracting = Animator.StringToHash("isInteracting");
    internal static readonly int hashCanRotate = Animator.StringToHash("canRotate");
    static readonly int hashIsAggressive = Animator.StringToHash("isAggressive");
    static readonly int hashIsCaution = Animator.StringToHash("isCaution");
    internal static readonly int hashMovement = Animator.StringToHash("movement");
    static readonly int hashGrabDeath = Animator.StringToHash("grab_death");
    static readonly int hashCaution = Animator.StringToHash("caution");
    static readonly int hashReload = Animator.StringToHash("Reload");
    static readonly int hashReloadBody = Animator.StringToHash("Reload_Body");
    static readonly int hashGrabStart = Animator.StringToHash("e_grab_start");
    static readonly int hashGrabCancel = Animator.StringToHash("e_grab_cancel");

    // ============================
    // Cached Values
    // ============================

    float cachedFovAngleCos;
    float sqrAttackDistance;

    // ============================
    // Lifecycle
    // ============================

    private void Awake()
    {
        patrolState = new PatrolState(this);
        cautionState = new CautionState(this);
        combatState = new CombatState(this);
        searchState = new SearchState(this);
        grabbedState = new GrabbedState(this);
    }

    private void Start()
    {
        hitSoundSource = GetComponent<AudioSource>();
        agent = GetComponentInChildren<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        inventoryManager = GetComponentInChildren<InventoryManager>();
        mTransform = transform;

        animator.applyRootMotion = false;
        controllerLayer = 1 << 9;
        ignoreForDetection = ~(1 << 12 | 1 << 13);
        currentHealth = maxHealth;

        cachedFovAngleCos = Mathf.Cos(fovAngle * Mathf.Deg2Rad);
        sqrAttackDistance = attackDistance * attackDistance;

        if (CurrentState == null)
            ChangeState(patrolState);
    }

    private void Update()
    {
        float delta = Time.deltaTime;

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        if (CurrentState == grabbedState)
            return;

        if (animator.GetBool(hashIsInteracting))
        {
            agent.isStopped = true;
            if (animator.GetBool(hashCanRotate))
                LookAtLastKnownPosition(delta);
            return;
        }

        animator.SetBool(hashIsAggressive, isAgressive);
        animator.SetBool(hashIsCaution, CurrentState == cautionState);

        bool wasAlerted = isAgressive;
        CurrentState.Tick(delta);

        if (wasAlerted)
            TickAlarm(delta);
    }

    // ============================
    // State Machine
    // ============================

    internal void ChangeState(AIState next)
    {
        if (next == CurrentState)
            return;

        AIState previous = CurrentState;
        previous?.Exit();
        CurrentState = next;
        currentStateName = next.GetType().Name;
        next.Enter(previous);
    }

    void TickAlarm(float delta)
    {
        if (alarmTimer > 0)
        {
            alarmTimer -= delta;
            return;
        }

        alarmTimer = 0;
        currentTarget = null;
        ChangeState(patrolState);
    }

    internal void EnterCaution(float duration, float delta, bool crossfadeToState = true)
    {
        cautionState.SetDuration(duration);

        if (crossfadeToState && CurrentState != grabbedState)
            animator.CrossFade(hashCaution, 0.2f);

        animator.SetFloat(hashMovement, 0, 0.1f, delta);
        ChangeState(cautionState);
    }

    void Die()
    {
        animator.Play(hashGrabDeath);
        isDead = true;
        enabled = false;
    }

    // ============================
    // Shared Helpers For States
    // ============================

    internal void LookAtLastKnownPosition(float delta)
    {
        RotateTowards(lastKnownPosition - mTransform.position, delta);
    }

    internal void RotateTowards(Vector3 dir, float delta)
    {
        dir.y = 0;
        if (dir == Vector3.zero)
            dir = mTransform.forward;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        mTransform.rotation = Quaternion.Slerp(mTransform.rotation, targetRot, delta / rotateSpeed);
        agent.updateRotation = false;
    }

    internal bool CanSeeTarget()
    {
        return currentTarget != null && RaycastToTarget(currentTarget);
    }

    // Called when the player breaks line of sight. Remembers which way they
    // were heading so the search starts by looking that way.
    internal void LoseTarget()
    {
        lastKnownDirection = (currentTarget.mTransform.position - lastKnownPosition).normalized;
        currentTarget = null;
    }

    internal void FireWeapon()
    {
        if (inventoryManager == null || inventoryManager.currentWeaponHook == null)
            return;

        GameReferences.RaycastShoot(mTransform, inventoryManager.currentWeaponHook, damageAmount);
        inventoryManager.currentWeaponHook.Shoot();
    }

    internal void PlayReload()
    {
        animator.CrossFade(hashReload, 0.2f);
        animator.CrossFade(hashReloadBody, 0.2f);
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

    internal void HandleDetection()
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
        alarmTimer = AlarmDuration;
        currentTarget = targetPlayer;
        lastKnownPosition = currentTarget.transform.position;
        SetToCautiousState();
    }

    public void SetToCautiousState(bool force = false)
    {
        if (isAgressive && !force)
            return;

        emotionText.text = "?!";
        emotionObj.SetActive(true);
        alarmTimer = AlarmDuration;

        // Switch state before alerting others: the alert also reaches this
        // guard, and being alerted already is what stops it from recursing.
        if (CurrentState == grabbedState)
            grabbedState.MarkAlerted();
        else
            EnterCaution(cautionTimerNormal, Time.deltaTime);

        GameReferences.UpdateLastKnownPositionOfCloseby(lastKnownPosition, 15);
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
        ChangeState(grabbedState);
        animator.Play(hashGrabStart);
        mTransform.rotation = targetRotation;

        emotionText.text = "?!";
        emotionObj.SetActive(true);

        GameReferences.UpdateLastKnownPositionOfCloseby(mTransform.position, 2);
    }

    public void KillByGrab()
    {
        Die();
    }

    public void StopGrab(Controller target)
    {
        currentTarget = target;
        lastKnownPosition = currentTarget.mTransform.position;
        agent.enabled = true;
        agent.updateRotation = true;
        animator.Play(hashGrabCancel);
        EnterCaution(cautionTimerNormal, Time.deltaTime, false);
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

        UpdateLastKnowPosition(mTransform.position);
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
