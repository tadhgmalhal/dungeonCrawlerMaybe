using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

public class PlayerMovement : MonoBehaviourPun
{
    [Header("Movement")]
    public float walkSpeed = 4f;
    public float sprintSpeed = 8f;
    public float mouseSensitivity = 2f;
    public float gravityForce = -20f;

    [Header("Jump")]
    public float jumpForce = 5f;
    public float coyoteTime = 0.15f;

    [Header("Stamina")]
    public float maxStamina = 100f;
    public float staminaDrainRate = 20f;
    public float staminaRegenWalking = 8f;
    public float staminaRegenIdle = 20f;
    public float heavyStaminaDrainMultiplier = 1.3f;

    private Rigidbody rb;
    private Transform cam;
    private PlayerControls controls;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private float verticalRotation = 0f;

    private bool isSprinting = false;
    private bool jumpPressed = false;

    private float currentStamina;
    private float coyoteTimer = 0f;
    private bool wasGrounded = false;
    private bool isJumping = false;

    //sack mechanic, to be added
    [HideInInspector] public int sackStage = 0;

    //footstep noise level, to be added
    [HideInInspector] public float noiseLevel = 0f;

    void Awake()
    {
        controls = new PlayerControls();

        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        controls.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        controls.Player.Look.canceled += ctx => lookInput = Vector2.zero;

        controls.Player.Sprint.performed += ctx => isSprinting = true;
        controls.Player.Sprint.canceled += ctx => isSprinting = false;

        controls.Player.Jump.performed += ctx => jumpPressed = true;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (!photonView.IsMine)
        {
            rb.isKinematic = true;
            return;
        }

        cam = GetComponentInChildren<Camera>().transform;
        Cursor.lockState = CursorLockMode.Locked;
        currentStamina = maxStamina;
    }

    void OnEnable()
    {
        if (controls != null) controls.Enable();
    }

    void OnDisable()
    {
        if (controls != null) controls.Disable();
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        verticalRotation -= lookInput.y * mouseSensitivity * Time.deltaTime * 100f;
        verticalRotation = Mathf.Clamp(verticalRotation, -80f, 80f);
        cam.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
        transform.Rotate(Vector3.up * lookInput.x * mouseSensitivity * Time.deltaTime * 100f);

        bool grounded = IsGrounded();
        if (grounded)
        {
            coyoteTimer = coyoteTime;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }

        if (jumpPressed && coyoteTimer > 0f)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            coyoteTimer = 0f;
            isJumping = true;
        }
        jumpPressed = false;

        HandleStamina(grounded);

        wasGrounded = grounded;
    }

    void HandleStamina(bool grounded)
    {
        bool isMoving = moveInput.magnitude > 0.1f;
        bool isHeavy = sackStage == 2;
        bool activelySprinting = isSprinting && currentStamina > 0f;

        if (activelySprinting)
        {
            float drain = staminaDrainRate * (isHeavy ? heavyStaminaDrainMultiplier : 1f);
            currentStamina -= drain * Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
        }
        else
        {
            // heavy sack can only regen while standing still
            if (isHeavy && isMoving)
            {
                
            }
            else if (isMoving)
            {
                currentStamina += staminaRegenWalking * Time.deltaTime;
            }
            else
            {
                currentStamina += staminaRegenIdle * Time.deltaTime;
            }

            currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
        }
    }

    void FixedUpdate()
    {
        if (!photonView.IsMine) return;

        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;

        float yVelocity;
        if (IsGrounded() && rb.linearVelocity.y <= -1f)
        {
            yVelocity = -0.5f;
            isJumping = false;
        }
        else
        {
            yVelocity = rb.linearVelocity.y + (gravityForce * Time.fixedDeltaTime);
            yVelocity = Mathf.Max(yVelocity, -20f);
        }

        bool activelySprinting = isSprinting && currentStamina > 0f && move.magnitude > 0.1f;

        float targetSpeed;
        if (activelySprinting)
        {
            if (sackStage == 1)
                targetSpeed = sprintSpeed * 0.5f;
            else if (sackStage == 2)
                targetSpeed = walkSpeed;
            else
                targetSpeed = sprintSpeed;
        }
        else
        {
            targetSpeed = walkSpeed;
        }

        rb.linearVelocity = new Vector3(move.x * targetSpeed, yVelocity, move.z * targetSpeed);

        if (!activelySprinting)
            noiseLevel = move.magnitude > 0.1f ? 1f : 0f;
        else
            noiseLevel = 5f;
    }

    bool IsGrounded()
    {
        CapsuleCollider col = GetComponent<CapsuleCollider>();
        float worldHalfHeight = (col.height * transform.localScale.y) / 2f;
        float rayLength = worldHalfHeight + 0.01f;
        return Physics.Raycast(transform.position, Vector3.down, rayLength, ~LayerMask.GetMask("Player"));
    }
}