using UnityEngine;
using UnityEngine.InputSystem;

public class lootItem : MonoBehaviour
{
    public string itemName = "Junk";
    public int junkValue = 10;
    public int magicalValue = 0;
    public float throwForce = 10f;
    public float pickupRange = 3f;
    public bool isDeposited = false;

    private bool isHeld = false;
    private Transform playerHand;
    private Rigidbody rb;
    private Outline outline;

    private float holdTimer = 0f;
    private bool qHeld = false;

    private static Camera mainCam;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        outline = GetComponent<Outline>();
        if (outline != null)
            outline.enabled = false;

        if (mainCam == null)
            mainCam = Camera.main;
    }

    private void Update()
    {
        if (isHeld)
        {
            HandleHeldInput();
        }
        else
        {
            CheckLookAt();
        }
    }

    private void CheckLookAt()
    {
        if (mainCam == null) return;

        Ray ray = new Ray(mainCam.transform.position, mainCam.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, pickupRange))
        {
            if (hit.collider.gameObject == gameObject)
            {
                if (outline != null) outline.enabled = true;

                if (Keyboard.current.eKey.wasPressedThisFrame)
                {
                    FindHand();
                    if (playerHand != null)
                        PickUp();
                }
                return;
            }
        }

        if (outline != null) outline.enabled = false;
    }

    private void FindHand()
    {
        Camera[] cams = FindObjectsOfType<Camera>();
        foreach (Camera cam in cams)
        {
            if (cam.name == "handCamera")
            {
                playerHand = cam.transform;
                break;
            }
        }

        if (playerHand == null)
            Debug.LogError("handCamera not found");
    }

    private void HandleHeldInput()
    {
        if (Keyboard.current.qKey.isPressed)
        {
            holdTimer += Time.deltaTime;
            qHeld = true;
        }

        if (qHeld && !Keyboard.current.qKey.isPressed)
        {
            if (holdTimer > 0.3f)
                Throw();
            else
                Drop();

            holdTimer = 0f;
            qHeld = false;
        }
    }

    private void PickUp()
    {
        isHeld = true;
        rb.isKinematic = true;
        foreach (Collider col in GetComponents<Collider>())
            col.enabled = false;
        if (outline != null) outline.enabled = false;
        transform.SetParent(playerHand);
        transform.localPosition = new Vector3(0.3f, -0.2f, 0.5f);
        transform.localRotation = Quaternion.identity;
    }

    public void Drop()
    {
        isHeld = false;
        transform.SetParent(null);
        rb.isKinematic = false;
        foreach (Collider col in GetComponents<Collider>())
            col.enabled = true;
    }

    public void Throw()
    {
        Vector3 throwDirection = playerHand.forward;
        transform.SetParent(null);
        isHeld = false;
        rb.isKinematic = false;
        foreach (Collider col in GetComponents<Collider>())
            col.enabled = true;
        rb.AddForce(throwDirection * throwForce, ForceMode.Impulse);
    }
}