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

        lootItem loot = other.GetComponent<lootItem>();
        if (loot != null && !loot.isDeposited)
        {
            loot.isDeposited = true;
            depositLoot(loot);
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

    private void depositLoot(lootItem loot)
    {
        stashManager.Instance.addLoot(loot.junkValue, loot.magicalValue);
        Debug.Log("Deposited " + loot.itemName + " — stash value: " + stashManager.Instance.totalJunkValue);
        Destroy(loot.gameObject);
    }

    private void goHome()
    {
        gameSceneManager.Instance.returnHome();
    }
}