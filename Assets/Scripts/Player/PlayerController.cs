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

    private float xRotation = 0f;
    private float yRotation = 0f;

    private CharacterController characterController;
    private Vector3 velocity;
    private bool isGrounded;
    private bool isCrouching;
    private float jumpCooldownTimer;
    private float currentHeight;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();

        currentHeight = normalHeight;
        characterController.height = normalHeight;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        HandleGroundCheck();
        HandleMovement();
        HandleCamera();
        HandleCrouch();
        HandleJump();
    }

    private void HandleGroundCheck()
    {
        // Check if player is touching ground using sphere cast
        Vector3 spherePosition = transform.position + Vector3.down * (characterController.height / 2f - 0.2f);
        isGrounded = Physics.CheckSphere(spherePosition, 0.3f);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        float currentSpeed = walkSpeed;

        // Sprint check
        if (Input.GetKey(KeyCode.LeftShift) && isGrounded && !isCrouching)
        {
            currentSpeed = sprintSpeed;
        }

        if (isCrouching)
        {
            currentSpeed = crouchSpeed;
        }

        Vector3 moveDirection = transform.forward * vertical + transform.right * horizontal;

        if (isGrounded)
        {
            characterController.Move(moveDirection * currentSpeed * Time.deltaTime);
        }
        else
        {
            characterController.Move(moveDirection * currentSpeed * airMultiplier * Time.deltaTime);
        }

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
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            isCrouching = !isCrouching;
        }

        float targetHeight = isCrouching ? crouchHeight : normalHeight;
        currentHeight = Mathf.Lerp(currentHeight, targetHeight, crouchTransitionSpeed * Time.deltaTime);
        characterController.height = currentHeight;

        Vector3 cameraPos = playerCamera.transform.localPosition;
        cameraPos.y = currentHeight * 0.6f;
        playerCamera.transform.localPosition = cameraPos;
    }

    private void HandleJump()
    {
        jumpCooldownTimer -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && jumpCooldownTimer <= 0 && !isCrouching)
        {
            velocity.y = jumpForce;
            jumpCooldownTimer = jumpCooldown;
        }
    }

    public float GetCurrentSpeed()
    {
        return new Vector3(characterController.velocity.x, 0, characterController.velocity.z).magnitude;
    }

    public bool IsGrounded()
    {
        return isGrounded;
    }

    public bool IsCrouching()
    {
        return isCrouching;
    }
}