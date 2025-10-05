using UnityEngine;

public class Collectible : MonoBehaviour
{
    [Header("VFX")]
    public GameObject destroyVFX;
    
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        // Get the SpriteRenderer component (either on this object or in children)
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        // Set the color to the theme's accent color
        if (spriteRenderer != null && ThemeController.Instance != null)
        {
            spriteRenderer.color = ThemeController.Instance.GetColor("accent");
        }
        else if (spriteRenderer == null)
        {
            Debug.LogWarning($"Collectible: No SpriteRenderer found on {name} or its children");
        }
    }

    public void Collect()
    {
        EventManager.Instance.TriggerEvent(EventManager.COLLECTIBLE_PICKED_UP, this);
        PlayerManager.Instance.OnCollectiblePickedUp(this);
        Destroy(gameObject);
    }

    /// <summary>
    /// Destroys the collectible with VFX effect
    /// </summary>
    public void DestroyMe()
    {
        // Spawn VFX if assigned
        if (destroyVFX != null)
        {
            GameObject vfxInstance = Instantiate(destroyVFX, transform.position, transform.rotation);
            // Optionally destroy the VFX after some time if it doesn't destroy itself
            // Destroy(vfxInstance, 5f);
        }
        
        // Destroy this collectible
        Destroy(gameObject);
    }
}
