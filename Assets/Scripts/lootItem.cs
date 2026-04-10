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
    private Transform holdTarget;
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
        {
            outline.enabled = false;
        }
        if (mainCam == null)
        {
            mainCam = Camera.main;
        }
    }

    private void Update()
    {
        if (isHeld)
        {
            transform.position = ItemHoldTarget.instance.transform.position;
            transform.rotation = ItemHoldTarget.instance.transform.rotation;
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
                    if (ItemHoldTarget.instance != null)
                    {
                        PickUp();
                    }
                }
                return;
            }
        }
        if (outline != null) outline.enabled = false;
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
            {
                Throw();
            }
            else
            {
                Drop();
            }
            holdTimer = 0f;
            qHeld = false;
        }
    }

    private void PickUp()
    {
        isHeld = true;
        rb.isKinematic = true;
        foreach (Collider col in GetComponents<Collider>())
        {
            col.enabled = false;
        }
        if (outline != null) outline.enabled = false;
        transform.SetParent(null);

        playerHP hp = mainCam.GetComponentInParent<playerHP>();
        if (hp != null)
        {
            hp.heldItem = this;
        }
    }

    public void Drop()
    {
        isHeld = false;
        rb.isKinematic = false;
        transform.localScale = Vector3.one;
        foreach (Collider col in GetComponents<Collider>())
        {
            col.enabled = true;
        }
        playerHP hp = mainCam.GetComponentInParent<playerHP>();
        if (hp != null)
        {
            hp.heldItem = null;
        }
    }

    public void Throw()
    {
        Vector3 throwDirection = ItemHoldTarget.instance.transform.forward;
        isHeld = false;
        rb.isKinematic = false;
        transform.localScale = Vector3.one;
        foreach (Collider col in GetComponents<Collider>())
        {
            col.enabled = true;
        }
        rb.AddForce(throwDirection * throwForce, ForceMode.Impulse);
        Vector3 randomTorque = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f)
        ) * throwForce * 0.2f;
        rb.AddTorque(randomTorque, ForceMode.Impulse);
        playerHP hp = mainCam.GetComponentInParent<playerHP>();
        if (hp != null)
        {
            hp.heldItem = null;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isHeld) return;
        if (collision.relativeVelocity.magnitude < 1f) return;
        Vector3 kickTorque = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f)
        ) * collision.relativeVelocity.magnitude * 0.3f;
        rb.AddTorque(kickTorque, ForceMode.Impulse);
    }
}