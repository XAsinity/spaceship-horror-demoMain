using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float crouchSpeed = 2.5f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float jumpCooldown = 0.25f;
    [SerializeField] private float airMultiplier = 0.4f;

    [Header("Camera")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float maxLookAngle = 90f;

    [Header("Crouch")]
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float normalHeight = 2f;
    [SerializeField] private float crouchTransitionSpeed = 5f;

    [Header("Ground Detection")]
    [SerializeField] private float groundCheckDistance = 0.5f;

    private float xRotation = 0f;
    private float yRotation = 0f;

    private CharacterController characterController;
    private Vector3 velocity;
    private bool isGrounded;
    private bool isCrouching;
    private float jumpCooldownTimer;
    private float currentHeight;

    // Debug timing
    private float debugLogTimer = 0f;
    private const float DEBUG_LOG_INTERVAL = 1f;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();

        currentHeight = normalHeight;
        characterController.height = normalHeight;
        Cursor.lockState = CursorLockMode.Locked;

        Debug.Log("[PlayerController] Initialized");
        Debug.Log($"[PlayerController] Character Controller Height: {characterController.height}");
    }

    private void Update()
    {
        HandleGroundCheck();
        HandleMovement();
        HandleCamera();
        HandleCrouch();
        HandleJump();
        UpdateDebugLog();
    }

    private void HandleGroundCheck()
    {
        // Simple ground check - cast down from player position
        Vector3 rayStart = transform.position;
        isGrounded = Physics.Raycast(rayStart, Vector3.down, characterController.height / 2f + groundCheckDistance);

        Debug.DrawRay(rayStart, Vector3.down * (characterController.height / 2f + groundCheckDistance), Color.green);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Debug input
        if ((horizontal != 0 || vertical != 0) && debugLogTimer <= 0)
        {
            Debug.Log($"[Movement] Input - H: {horizontal}, V: {vertical}, Grounded: {isGrounded}");
            debugLogTimer = DEBUG_LOG_INTERVAL;
        }

        // Only allow movement if grounded
        if (!isGrounded)
        {
            if (debugLogTimer <= 0)
            {
                Debug.Log("[Movement] Cannot move - not grounded!");
                debugLogTimer = DEBUG_LOG_INTERVAL;
            }
            return;
        }

        float currentSpeed = walkSpeed;

        // Sprint check
        if (Input.GetKey(KeyCode.LeftShift) && !isCrouching)
        {
            currentSpeed = sprintSpeed;
        }

        if (isCrouching)
        {
            currentSpeed = crouchSpeed;
        }

        Vector3 moveDirection = transform.forward * vertical + transform.right * horizontal;
        characterController.Move(moveDirection * currentSpeed * Time.deltaTime);

        velocity.y += Physics.gravity.y * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    private void HandleCamera()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        yRotation += mouseX;
        transform.localRotation = Quaternion.Euler(0f, yRotation, 0f);

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private void HandleCrouch()
    {
        // Only allow crouch if grounded
        if (!isGrounded)
            return;

        if (Input.GetKeyDown(KeyCode.C))
        {
            isCrouching = !isCrouching;
            Debug.Log($"[Crouch] Toggled: {isCrouching}");
        }

        float targetHeight = isCrouching ? crouchHeight : normalHeight;
        currentHeight = Mathf.Lerp(currentHeight, targetHeight, crouchTransitionSpeed * Time.deltaTime);
        characterController.height = currentHeight;
    }

    private void HandleJump()
    {
        jumpCooldownTimer -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && jumpCooldownTimer <= 0f)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * Physics.gravity.y);
            jumpCooldownTimer = jumpCooldown;
            Debug.Log("[Jump] Jumped!");
        }
    }

    private void UpdateDebugLog()
    {
        debugLogTimer -= Time.deltaTime;

        if (debugLogTimer <= 0)
        {
            Debug.Log($"[Status] Grounded: {isGrounded}, Position: {transform.position}");
            debugLogTimer = DEBUG_LOG_INTERVAL;
        }
    }
}