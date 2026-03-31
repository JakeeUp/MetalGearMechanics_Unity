using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InputHandler : MonoBehaviour
{
    // ============================
    // Serialized Fields
    // ============================

    public Transform camHolder;
    public ExecutionOrder movementOrder;
    public Controller controller;
    public CameraManager cameraManager;
    public Transform wallCameraTarget;

    // ============================
    // Movement Config
    // ============================

    public float wallDetectDis = .5f;
    public float wallDetectDisOnWall = 1.2f;
    public float wallAngleThreshold = 35;

    // ============================
    // State
    // ============================

    Vector3 moveDirection;
    Vector2 moveInputDirection;
    float horizontal;
    float vertical;
    float moveAmount;
    bool freeLook;
    bool grabInput;
    bool isFPSinit;
    float grabDeadTimer;
    LayerMask ignoreForWall;
    PlayerControls inputActions;

    public enum ExecutionOrder { fixedUpdate, update, lateUpdate }

    // ============================
    // Lifecycle
    // ============================

    private void Start()
    {
        inputActions = new PlayerControls();
        inputActions.Player.Movement.performed += ctx => moveInputDirection = ctx.ReadValue<Vector2>();
        inputActions.Enable();

        cameraManager.wallCameraObject.SetActive(false);
        cameraManager.mainCameraObject.SetActive(true);
        cameraManager.fpsCameraObject.SetActive(false);
        cameraManager.mainCamera.cullingMask = ~0;

        ignoreForWall = ~(1 << 9 | 1 << 12 | 1 << 13);
        GameReferences.ignoreForShooting = ~(1 << 12 | 1 << 13);
        GameReferences.controllersLayer = 1 << 9;

        if (controller.inventoryManager != null)
            UIManager.singleton.Init(controller.inventoryManager);

        List<Jacob.Utilities.IIcon> iconList = new List<Jacob.Utilities.IIcon>();
        iconList.AddRange(ResourcesManager.singleton.GetAllItems());
        Jacob.Utilities.IconMaker.RequestIconForList(iconList, UpdateUIManagerWithItems);
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    void UpdateUIManagerWithItems()
    {
        List<Item> items = new List<Item>();
        items.AddRange(ResourcesManager.singleton.GetAllItems());
        UIManager.singleton.CreateSlotsForItemList(items);
    }

    // ============================
    // Update Loop
    // ============================

    private void FixedUpdate()
    {
        if (movementOrder == ExecutionOrder.fixedUpdate)
            HandleMovement(moveDirection, Time.fixedDeltaTime);
    }

    private void Update()
    {
        float delta = Time.deltaTime;

        bool isLeftBumperPressed = Input.GetKey(KeyCode.LeftControl);
        bool isInventory = UIManager.singleton.Tick(moveInputDirection.y, delta, isLeftBumperPressed, false);

        if (isInventory)
            return;

        GatherInput(delta);
        HandleStateTransitions(delta);
    }

    // ============================
    // Input Gathering
    // ============================

    private void GatherInput(float delta)
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        controller.isAiming = Input.GetMouseButton(1);
        freeLook = Input.GetKey(KeyCode.F);

        bool rawGrabInputHold = Input.GetMouseButton(0);
        bool rawGrabInputDown = Input.GetMouseButtonDown(0);
        bool doubleGrab = false;
        bool switchWeapon = Input.GetKeyDown(KeyCode.Q);

        if (Input.GetKey(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SceneManager.LoadScene("MainMenuScene");
        }

        if (rawGrabInputHold)
        {
            grabInput = true;
            if (grabDeadTimer > 0)
                doubleGrab = true;
            grabDeadTimer = 0;
        }
        else
        {
            grabDeadTimer += delta;
            if (grabDeadTimer > 1)
                grabInput = false;
        }

        controller.isInteracting = controller.animator.GetBool("isInteracting");

        if (switchWeapon && controller.inventoryManager != null)
            controller.inventoryManager.SwitchWeapon();

        if (controller.isAiming)
        {
            grabInput = false;
            freeLook = false;
        }

        if (grabInput)
            freeLook = false;

        HandleFreeLookCamera(delta);

        if (controller.isInteracting)
        {
            controller.rigidbody.velocity = Vector3.zero;
            return;
        }

        controller.HandleGrab(grabInput, doubleGrab, rawGrabInputDown);
    }

    // ============================
    // Camera Modes
    // ============================

    private void HandleFreeLookCamera(float delta)
    {
        if (freeLook)
        {
            float rotationSpeed = 50f;
            if (Input.GetKey(KeyCode.O))
                camHolder.Rotate(Vector3.up, -rotationSpeed * delta, Space.Self);
            if (Input.GetKey(KeyCode.P))
                camHolder.Rotate(Vector3.up, rotationSpeed * delta, Space.Self);

            if (!controller.isFreeLook)
            {
                controller.isFreeLook = true;
                controller.isAiming = false;
                controller.isProne = false;
                cameraManager.fpsCameraObject.SetActive(true);
                controller.rigidbody.velocity = Vector3.zero;
                cameraManager.mainCamera.cullingMask = ~(1 << 10);
            }
        }
        else
        {
            if (controller.isFreeLook)
            {
                controller.isFreeLook = false;
                cameraManager.fpsCameraObject.SetActive(false);
                cameraManager.mainCamera.cullingMask = ~0;
            }
        }
    }

    // ============================
    // State Transitions
    // ============================

    private void HandleStateTransitions(float delta)
    {
        if (controller.isFPS)
        {
            HandleFPSMovement(delta);
            return;
        }

        isFPSinit = false;
        moveAmount = Mathf.Clamp01(Mathf.Abs(moveInputDirection.x) + Mathf.Abs(moveInputDirection.y));
        moveDirection = Vector3.forward * moveInputDirection.y + Vector3.right * moveInputDirection.x;
        moveDirection.Normalize();

        if (Input.GetKeyDown(KeyCode.C))
        {
            controller.isCrouch = !controller.isCrouch;
            controller.UpdatePoseStats(controller.isCrouch ? controller.crouching : controller.standing);

            if (!controller.isWall)
                moveDirection = Vector3.zero;
        }

        if (controller.isFreeLook)
        {
            controller.FPSRotate(horizontal, delta);
        }
        else
        {
            HandleThirdPersonCombatAndMovement(delta);
        }

        controller.HandleAnimationStates();
    }

    private void HandleFPSMovement(float delta)
    {
        if (!isFPSinit)
        {
            cameraManager.fpsCameraObject.SetActive(true);
            cameraManager.mainCamera.cullingMask = ~(1 << 7);
            isFPSinit = true;
        }

        moveDirection = controller.mTransform.forward * moveInputDirection.y
                      + controller.mTransform.right * moveInputDirection.x;
        moveDirection.Normalize();

        controller.FPSRotate(horizontal, delta);
        controller.Move(moveDirection, delta);
    }

    private void HandleThirdPersonCombatAndMovement(float delta)
    {
        if (controller.inventoryManager.currentWeaponHook == null)
            controller.isAiming = false;

        if (controller.isAiming)
        {
            controller.isCrouch = false;
            controller.HandleRotation(moveDirection, delta);

            if (Input.GetMouseButton(0))
                controller.HandleShooting();

            if (controller.inventoryManager.currentWeapon != null && controller.inventoryManager.currentWeapon.canMoveWithWeapon)
            {
                controller.Move(moveDirection, delta);
                controller.HandleMovementAnimations(moveAmount, delta);
            }
            else
            {
                controller.HandleMovementAnimations(0, delta);
                controller.rigidbody.velocity = Vector3.zero;
            }
        }
        else
        {
            if (movementOrder == ExecutionOrder.update)
                HandleMovement(moveDirection, delta);
        }
    }

    // ============================
    // Movement Handler
    // ============================

    void HandleMovement(Vector3 moveDir, float delta)
    {
        if (controller.isGrab)
        {
            controller.HandleGrabAnimation(moveAmount, delta);

            if (moveAmount == 1)
            {
                controller.GrabMove(moveDir, delta);
                controller.HandleRotation(-moveDir, delta);
            }

            controller.HandleEnemyPositionOnGrab();
            return;
        }

        Vector3 origin = controller.transform.position;
        origin.y += controller.getWallDetectOrigin;

        bool willStickToWall = false;
        Vector3 wallNormal = Vector3.zero;
        float detectDis = controller.isWall ? wallDetectDisOnWall : wallDetectDis;

        Debug.DrawRay(origin, moveDir * detectDis);

        if (Physics.SphereCast(origin, 0.25f, moveDir, out RaycastHit hit, detectDis, ignoreForWall))
        {
            willStickToWall = true;
            wallNormal = hit.normal;
        }

        if (willStickToWall)
        {
            wallCameraTarget.transform.position = controller.transform.position;
            wallCameraTarget.transform.rotation = Quaternion.LookRotation(wallNormal);
            controller.isProne = false;
            controller.isWall = true;
            controller.WallMovement(moveDir, wallNormal, delta, ignoreForWall);
            cameraManager.wallCameraObject.SetActive(true);
            cameraManager.mainCameraObject.SetActive(false);
        }
        else
        {
            controller.isWall = false;
            cameraManager.wallCameraObject.SetActive(false);
            cameraManager.mainCameraObject.SetActive(true);

            if (controller.isCrouch)
            {
                controller.CrouchMovement(moveDir, delta, moveAmount);
            }
            else
            {
                controller.Move(moveDir, delta);
                controller.HandleRotation(moveDir, delta);
                controller.HandleMovementAnimations(moveAmount, delta);
            }
        }
    }
}
