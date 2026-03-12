using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 4f;
    public float mouseSensitivity = 2f;
    public float gravityForce = -20f;

    private Rigidbody rb;
    private Transform cam;
    private PlayerControls controls;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private float verticalRotation = 0f;

    void Awake()
    {
        controls = new PlayerControls();

        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        controls.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        controls.Player.Look.canceled += ctx => lookInput = Vector2.zero;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cam = GetComponentInChildren<Camera>().transform;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void OnEnable() => controls.Enable();
    void OnDisable() => controls.Disable();

    void Update()
    {
        verticalRotation -= lookInput.y * mouseSensitivity * Time.deltaTime * 100f;
        verticalRotation = Mathf.Clamp(verticalRotation, -80f, 80f);

        cam.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
        transform.Rotate(Vector3.up * lookInput.x * mouseSensitivity * Time.deltaTime * 100f);
    }

    void FixedUpdate()
    {
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        Vector3 velocity = move * moveSpeed + Vector3.up * rb.linearVelocity.y;
        rb.linearVelocity = velocity;

        if (!IsGrounded())
            rb.AddForce(Vector3.up * gravityForce, ForceMode.Acceleration);
    }

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 1.1f);
    }
}