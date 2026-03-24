using UnityEngine;
using UnityEngine.InputSystem;

public class lobbyPortal : MonoBehaviour
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
            gameStart();
        }
    }

    private void gameStart()
    {
        gameSceneManager.Instance.descendToNextFloor();
    }
}