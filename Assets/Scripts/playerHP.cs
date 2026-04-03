using UnityEngine;

public class playerHP : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;
    private spectatorCam specCam;
    [HideInInspector] public lootItem heldItem;

    private void Start()
    {
        currentHealth = maxHealth;
        specCam = GetComponent<spectatorCam>();
    }

    public void takeDamage(float amount)
    {
        if (currentHealth <= 0)
            return;

        currentHealth -= amount;
        Debug.Log("Health: " + currentHealth);

        if (currentHealth <= 0)
        {
            die();
        }
    }

    public void heal(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        Debug.Log("Healed to: " + currentHealth);
    }

    private void die()
    {
        Debug.Log("Goblin died");

        // apply stash penalty
        if (stashManager.Instance != null)
            stashManager.Instance.applyDeathPenalty();

        // disable player control
        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null)
            movement.enabled = false;

        if (heldItem != null)
        {
            heldItem.Drop();
            heldItem = null;
        }
        Debug.Log("Items dropped");

        // hide goblin model
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
            r.enabled = false;

        // trigger spectator camera -- next step
        Debug.Log("Switch to spectator cam");
        if (specCam != null)
            specCam.startSpectating();
    }
}