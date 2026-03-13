using UnityEngine;
using UnityEngine.InputSystem;

public class dungeonPortal : MonoBehaviour
{
    private bool playerInside = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            Debug.Log("Press E to go home");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
        }
    }

    private void Update()
    {
        if (!playerInside) return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            goHome();
        }
    }

    private void goHome()
    {
        Debug.Log("Going home");
        // bank stash and load cave scene here later
    }
}