using UnityEngine;
using UnityEngine.SceneManagement;

public class Controller : MonoBehaviour, IShootable, IPointOfInterest
{
    // ============================
    // Health
    // ============================

    [Header("Attributes")]
    [Space(5)]
    public float maxHealth = 100f;
    [SerializeField] private float _currentHealth;

    public float currentHealth
    {
        get { return _currentHealth; }
        set { _currentHealth = value; }
    }

    // ============================
    // Movement
    // ============================

    [Header("Movement")]
    [Space(5)]
    [SerializeField] private float _moveSpeed = 6f;
    [SerializeField] private float _grabSpeed = .8f;
    [SerializeField] private float _proneSpeed = 1.2f;
    [SerializeField] private float _wallSpeed = 1f;
    [SerializeField] private float _rotateSpeed = .1f;
    [SerializeField] private float _fpsRotateSpeed = .01f;
    [SerializeField] private float _wallCheckDis = .7f;

    // ============================
    // Combat
    // ============================

    [Header("Attacking")]
    [Space(5)]
    public float aimSpeed = 1;
    [HideInInspector] public Transform mTransform;
    [HideInInspector] public InventoryManager inventoryManager;
    public Animator animator;
    public float dmgNumber;

    // ============================
    // State Flags
    // ============================

    [Header("Bools")]
    [Space(5)]
    [SerializeField] private bool _isWall;
    [SerializeField] private bool _isAiming;
    [SerializeField] private bool _isFreeLook;
    [SerializeField] private bool _isGrab;
    [SerializeField] private bool _isInteracting;
    [SerializeField] private bool _isFPS;
    [SerializeField] private bool _isProne;
    bool _isCrouch;

    public bool isCrouch
    {
        get { return _isCrouch; }
        set
        {
            animator.SetBool("isProne", false);
            _isCrouch = value;
        }
    }

    // ============================
    // Audio
    // ============================

    [Header("Sound")]
    [Space(5)]
    public AudioSource spottedSoundSource;
    public AudioClip spottedSound;
    public static float timeSinceLastPlay = 0f;

    // ============================
    // References
    // ============================

    [Header("Other")]
    [Space(5)]
    public AIController enemy;
    AIController currentGrabbed;
    public PoseStats standing;
    public PoseStats crouching;
    CapsuleCollider controllerCollider;

    [HideInInspector] public GameObject storedObject;
    [HideInInspector] public Animator boxAnimator;
    public new Rigidbody rigidbody;
    public float wallCamXPos = 1;
    public Transform wallCamParent;
    public Vector3 startWallCamPos;
    public SkinnedMeshRenderer meshRenderer;

    // ============================
    // State
    // ============================

    public enum ControllerState { normal, cardboardBox, prone }
    public ControllerState controllerState;
    public string hitFx = "blood";
    public float grabOffset;
    public float grabDistance = 1;
    public AudioClip gruntSound;
    float lastShot;

    // ============================
    // Properties
    // ============================

    public float getWallDetectOrigin
    {
        get { return isCrouch ? crouching.wallDetectHeight : standing.wallDetectHeight; }
    }

    public float moveSpeed { get { return _moveSpeed; } set { _moveSpeed = value; } }
    public float grabSpeed { get { return _grabSpeed; } set { _grabSpeed = value; } }
    public float proneSpeed { get { return _proneSpeed; } set { _proneSpeed = value; } }
    public float wallSpeed { get { return _wallSpeed; } set { _wallSpeed = value; } }
    public float rotateSpeed { get { return _rotateSpeed; } set { _rotateSpeed = value; } }
    public float fpsRotateSpeed { get { return _fpsRotateSpeed; } set { _fpsRotateSpeed = value; } }
    public float wallCheckDis { get { return _wallCheckDis; } set { _wallCheckDis = value; } }
    public bool isWall { get { return _isWall; } set { _isWall = value; } }
    public bool isAiming { get { return _isAiming; } set { _isAiming = value; } }
    public bool isFreeLook { get { return _isFreeLook; } set { _isFreeLook = value; } }
    public bool isGrab { get { return _isGrab; } set { _isGrab = value; } }
    public bool isInteracting { get { return _isInteracting; } set { _isInteracting = value; } }
    public bool isFPS { get { return _isFPS; } set { _isFPS = value; } }
    public bool isProne { get { return _isProne; } set { _isProne = value; } }

    // ============================
    // Lifecycle
    // ============================

    private void Start()
    {
        mTransform = transform;
        rigidbody = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        inventoryManager = GetComponentInParent<InventoryManager>();
        controllerCollider = GetComponent<CapsuleCollider>();
        startWallCamPos = wallCamParent.localPosition;
        meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        enemy = FindObjectOfType<AIController>();
        spottedSoundSource = GetComponent<AudioSource>();

        currentHealth = maxHealth;
        UpdatePoseStats(standing);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleDeath();
        timeSinceLastPlay += Time.deltaTime;
    }

    // ============================
    // Health & Death
    // ============================

    private void HandleDeath()
    {
        if (currentHealth <= 0)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SceneManager.LoadScene("DeathScene");
        }
    }

    // ============================
    // Movement
    // ============================

    public void Move(Vector3 moveDirection, float delta)
    {
        if (animator.GetBool("canRotate"))
            moveDirection = Vector3.zero;

        float speed = isAiming ? aimSpeed : moveSpeed;
        rigidbody.velocity = moveDirection * speed;
    }

    public void GrabMove(Vector3 moveDirection, float delta)
    {
        rigidbody.velocity = moveDirection * grabSpeed;
    }

    public void WallMovement(Vector3 moveDirection, Vector3 normal, float delta, LayerMask layerMask)
    {
        float dot = Vector3.Dot(moveDirection, Vector3.forward);
        Vector3 wallCamTargetPos = startWallCamPos;

        if (dot < 0)
            moveDirection.x *= -1;

        HandleRotation(normal, delta);

        Vector3 projectVel = Vector3.ProjectOnPlane(moveDirection, normal);
        Debug.DrawRay(mTransform.position, projectVel, Color.blue);

        Vector3 relativeDir = mTransform.InverseTransformDirection(projectVel);
        Vector3 origin = mTransform.position;
        origin.y += 1;

        if (Mathf.Abs(relativeDir.x) > 0.01f)
        {
            if (relativeDir.x > 0)
                origin += mTransform.right * wallCheckDis;
            else
                origin -= mTransform.right * wallCheckDis;

            Debug.DrawRay(origin, -normal, Color.red);

            if (!Physics.Raycast(origin, -normal, out RaycastHit hit, 2, layerMask))
            {
                projectVel = Vector3.zero;
                wallCamTargetPos.x = wallCamXPos * (relativeDir.x < 0 ? -1 : 1);
                relativeDir.x = 0;
            }
        }
        else
        {
            projectVel = Vector3.zero;
            relativeDir.x = 0;
        }

        rigidbody.velocity = projectVel * wallSpeed;

        float m = relativeDir.x;
        if (m < 0.1f && m > -0.1f)
            m = 0;
        else
            m = m < 0 ? -1 : 1;

        animator.SetFloat("movement", m, 0.1f, delta);
        wallCamParent.localPosition = Vector3.Lerp(wallCamParent.localPosition, wallCamTargetPos, delta / 0.2f);
    }

    public void CrouchMovement(Vector3 moveDirection, float delta, float moveAmount)
    {
        float dot = Vector3.Dot(moveDirection, mTransform.forward);
        HandleMovementAnimations(moveAmount, delta);

        if (dot > 0)
        {
            Debug.DrawRay(mTransform.position, moveDirection);
            rigidbody.velocity = moveDirection * proneSpeed;

            if (moveAmount > 0)
            {
                isProne = true;
                HandleRotation(moveDirection, delta);
                animator.SetBool("canRotate", false);
            }
        }
        else
        {
            if (moveAmount > 0)
            {
                isProne = false;
                if (animator.GetBool("canRotate"))
                {
                    rigidbody.velocity = Vector3.zero;
                    HandleRotation(moveDirection, delta);
                }
            }
        }
    }

    // ============================
    // Rotation
    // ============================

    public void HandleRotation(Vector3 lookDir, float delta)
    {
        if (lookDir == Vector3.zero)
            lookDir = mTransform.forward;

        Quaternion lookRotation = Quaternion.LookRotation(lookDir);
        mTransform.rotation = Quaternion.Slerp(mTransform.rotation, lookRotation, delta / rotateSpeed);
    }

    public void FPSRotate(float horizontal, float delta)
    {
        Vector3 targetEuler = mTransform.eulerAngles;
        targetEuler.y += horizontal * delta / fpsRotateSpeed;
        mTransform.eulerAngles = targetEuler;
    }

    // ============================
    // Animation
    // ============================

    public void HandleAnimationStates()
    {
        animator.SetBool("isCrouch", isCrouch);
        animator.SetBool("isWall", isWall);
        animator.SetBool("isAiming", isAiming);
        animator.SetBool("isProne", isProne);

        if (inventoryManager.currentWeaponHook != null)
            inventoryManager.currentWeaponHook.gameObject.SetActive(isAiming);
    }

    public void HandleMovementAnimations(float moveAmount, float delta)
    {
        float m = moveAmount;
        if (m > 0.1f && m < 0.51f) m = 0.5f;
        else if (m > 0.51f) m = 1;
        else if (m < 0.1f) m = 0;

        switch (controllerState)
        {
            case ControllerState.cardboardBox:
                boxAnimator.SetFloat("movement", m, 0.1f, delta);
                break;
        }

        animator.SetFloat("movement", m, 0.1f, delta);
    }

    public void HandleGrabAnimation(float moveAmount, float delta)
    {
        animator.SetFloat("movement", moveAmount, 0.1f, delta);
        currentGrabbed.animator.SetFloat("movement", moveAmount, 0.1f, delta);
    }

    // ============================
    // Combat
    // ============================

    public void HandleShooting()
    {
        if (inventoryManager.currentWeapon == null || inventoryManager.currentWeaponHook == null)
            return;

        if (Time.realtimeSinceStartup - lastShot > inventoryManager.currentWeapon.fireRate)
        {
            lastShot = Time.realtimeSinceStartup;
            inventoryManager.currentWeaponHook.Shoot();
            GameReferences.RaycastShoot(mTransform, inventoryManager.currentWeaponHook);
        }
    }

    // ============================
    // Grab System
    // ============================

    public void HandleGrab(bool isHolding, bool doubleGrab, bool isTrigger)
    {
        if (currentGrabbed != null && doubleGrab && !isInteracting)
        {
            spottedSoundSource.clip = gruntSound;
            spottedSoundSource.PlayOneShot(gruntSound);
            animator.Play("p_grab_struggle");
            currentGrabbed.animator.Play("e_grab_struggle");
            currentGrabbed.timesStruggle++;

            if (currentGrabbed.timesStruggle > 2)
            {
                isGrab = false;
                animator.Play("p_grab_finish");
                currentGrabbed.KillByGrab();
                currentGrabbed = null;
                return;
            }
        }

        if (isHolding)
        {
            if (currentGrabbed == null && isTrigger)
                TryGrabEnemy();
        }
        else
        {
            if (currentGrabbed != null)
            {
                isGrab = false;
                animator.Play("p_grab_cancel");
                currentGrabbed.StopGrab(this);
                currentGrabbed = null;
            }
        }
    }

    private void TryGrabEnemy()
    {
        Vector3 origin = mTransform.position;
        origin.y += 1.5f;
        rigidbody.velocity = Vector3.zero;

        Debug.DrawRay(origin, mTransform.forward * grabDistance, Color.blue, 1, false);

        if (Physics.SphereCast(origin, 0.25f, mTransform.forward, out RaycastHit hit, grabDistance))
        {
            AIController aIController = hit.transform.GetComponentInParent<AIController>();

            if (aIController != null && !aIController.isDead)
            {
                Vector3 tp = mTransform.forward * grabOffset + mTransform.position;
                aIController.StartGrab(tp, mTransform.rotation);
                animator.Play("p_grab_start");
                isGrab = true;
                currentGrabbed = aIController;
                return;
            }
        }

        animator.Play("p_grab_empty");
    }

    public void HandleEnemyPositionOnGrab()
    {
        Vector3 tp = mTransform.forward * grabOffset + mTransform.position;
        currentGrabbed.transform.position = tp;
        currentGrabbed.transform.rotation = mTransform.rotation;
    }

    // ============================
    // IShootable Implementation
    // ============================

    public void OnHit(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);
    }

    public string GetHitFx()
    {
        return hitFx;
    }

    // ============================
    // Detection
    // ============================

    public void DetectSound()
    {
        if (enemy != null && enemy.isAgressive)
            spottedSoundSource.PlayOneShot(spottedSound);
    }

    public bool OnDetect(AIController aIController)
    {
        if (controllerState == ControllerState.cardboardBox)
        {
            if (rigidbody.velocity.sqrMagnitude > 0.1f)
                aIController.OnDetectPlayer(this);
            else
                return false;
        }
        else
        {
            aIController.OnDetectPlayer(this);
        }

        return true;
    }

    public Transform GetTransform()
    {
        return mTransform;
    }

    // ============================
    // Pose System
    // ============================

    public void UpdatePoseStats(PoseStats pose)
    {
        controllerCollider.height = pose.colliderHeight;
        Vector3 centerPosition = controllerCollider.center;
        centerPosition.y = pose.colliderPosY;
        controllerCollider.center = centerPosition;
    }

    // ============================
    // Nested Types
    // ============================

    [System.Serializable]
    public class PoseStats
    {
        public float colliderHeight = 2.7f;
        public float colliderPosY = 1.3f;
        public float wallDetectHeight;
    }
}
