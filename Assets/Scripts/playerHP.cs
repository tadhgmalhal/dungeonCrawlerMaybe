using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.InputSystem;

public class playerHP : MonoBehaviourPun
{
    public float maxHealth = 100f;
    public float iframeDuration = 0.5f;

    [Header("Vignette")]
    public Image vignetteImage;
    public float vignetteThreshold = 0.5f;

    private float currentHealth;
    private float iframeTimer = 0f;
    private spectatorCam specCam;
    [HideInInspector] public lootItem heldItem;

    private void Start()
    {
        currentHealth = maxHealth;
        specCam = GetComponent<spectatorCam>();

        if (vignetteImage != null)
        {
            int size = 256;
            Texture2D tex = new Texture2D(size, size);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x / (float)size) - 0.5f;
                    float dy = (y / (float)size) - 0.5f;
                    float dist = Mathf.Clamp01(Mathf.Sqrt(dx * dx + dy * dy) * 2f);
                    tex.SetPixel(x, y, new Color(1f, 0f, 0f, dist));
                }
            }
            tex.Apply();
            vignetteImage.sprite = Sprite.Create(tex, new Rect(0, 0, size, size), Vector2.one * 0.5f);
        }
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        if (iframeTimer > 0f)
        {
            iframeTimer -= Time.deltaTime;
        }

        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            takeDamage(40f);
        }

        updateVignette();
    }

    private void updateVignette()
    {
        if (vignetteImage == null) return;

        float healthPercent = currentHealth / maxHealth;
        if (healthPercent < vignetteThreshold)
        {
            float alpha = Mathf.InverseLerp(vignetteThreshold, 0f, healthPercent);
            vignetteImage.color = new Color(.3f, 0f, 0f, alpha * .7f);
        }
        else
        {
            vignetteImage.color = new Color(1f, 0f, 0f, 0f);
        }
    }

    public void takeDamage(float amount)
    {
        if (!photonView.IsMine) return;
        if (currentHealth <= 0) return;
        if (iframeTimer > 0f) return;

        currentHealth -= amount;
        iframeTimer = iframeDuration;

        if (currentHealth <= 0)
        {
            die();
        }
    }

    public void heal(float amount)
    {
        if (!photonView.IsMine) return;
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
    }

    private void die()
    {
        if (stashManager.Instance != null)
        {
            stashManager.Instance.applyDeathPenalty();
        }

        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.enabled = false;
        }

        if (heldItem != null)
        {
            heldItem.Drop();
            heldItem = null;
        }

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            r.enabled = false;
        }

        if (specCam != null)
        {
            specCam.startSpectating();
        }
    }
}