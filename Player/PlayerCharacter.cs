using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;


public class PlayerCharacter : Character
{
    public GameObject PlayerGraphics;
    public MeshRenderer meshRenderer;
    public SpriteFlicker spriteFlicker;

    [HideInInspector] public Rigidbody2D rb;


    public SpriteRenderer spriteRenderer;
    [HideInInspector] public SpriteTrails trails;

    public LayerMask obstacleLayer;

    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!spriteRenderer) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (!trails) trails = GetComponentInChildren<SpriteTrails>();
        if (!spriteFlicker) spriteFlicker = GetComponentInChildren<SpriteFlicker>();

        // Set the sprite renderer color to the theme's primary light color
        if (spriteRenderer != null && ThemeController.Instance != null)
        {
            spriteRenderer.color = ThemeController.Instance.GetColor("primaryLight");
        }
        else if (spriteRenderer == null)
        {
            Debug.LogWarning($"PlayerCharacter: No SpriteRenderer found on {name} or its children");
        }

        if (PlayerManager.Instance) PlayerManager.Instance.UpdatePlayerRef(this);

        startingMaterial = spriteRenderer.material;
    }

    void Update()
    {
        if (!GameManager.Instance.IsGameRunning() || GameManager.Instance.IsGamePaused()) return;

        HandleAnimation();
    }

    void HandleAnimation()
    {
        if (!anim || !rb) return;

        anim.SetFloat("velocityX", rb.velocity.x);
        anim.SetFloat("velocityY", rb.velocity.y);

        anim.SetBool("isMoving", PlayerManager.Instance.isMoving);
        anim.SetBool("isHurt", PlayerManager.Instance.State == PlayerState.Hurt);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            //PlayerManager.Instance.PlayerHit(Combat.Instance.contactDamage);
        }
    }
}
