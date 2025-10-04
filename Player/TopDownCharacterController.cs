using System.Collections;
using UnityEngine;

public class TopDownCharacterController : MonoBehaviour
{
    [ReadOnly] public Vector2 direction;
    [HideInInspector] public Rigidbody2D rb;
    public bool isMoving;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (PlayerManager.Instance.State == PlayerState.Dead || PlayerManager.Instance.State == PlayerState.Hurt || GameManager.Instance.gamePaused || !PlayerManager.Instance.canMove) return;

        direction = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        //CheckIfMoving();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        rb.velocity = PlayerManager.Instance.canMove && PlayerManager.Instance.State != PlayerState.Dead && !GameManager.Instance.gamePaused
        ? direction * PlayerManager.Instance.MoveSpeed
        : Vector2.zero;
    }

    //private void CheckIfMoving()
    //{
    //    if (rb.velocity.magnitude > GameManager.Instance.velocityThreshold)
    //    {
    //        if (!isMoving)
    //        {
    //            isMoving = true;
    //            StopAllCoroutines();
    //            StartCoroutine(GameManager.Instance.TimeAcceleration());
    //        }
    //    }
    //    else
    //    {
    //        if (isMoving)
    //        {
    //            isMoving = false;
    //            StopAllCoroutines();
    //            StartCoroutine(GameManager.Instance.TimeDeceleration());
    //        }
    //    }
    //}

}
