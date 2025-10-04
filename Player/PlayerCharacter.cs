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

        if (PlayerManager.Instance) PlayerManager.Instance.UpdatePlayerRef(this);

        startingMaterial = spriteRenderer.material;
    }

    void Update()
    {
        if (!GameManager.Instance.gameRunning || GameManager.Instance.gamePaused) return;

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
