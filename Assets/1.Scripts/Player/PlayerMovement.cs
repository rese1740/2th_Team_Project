using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Settings")]
    public  bool isGrounded;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    private bool facingRight = true;

    [Header("Components")]
    public PlayerSO playerData;
    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;

    private void Start()
    {
       rb = GetComponent<Rigidbody2D>();
        playerData.Init();
    }

    private void Update()
    {
        if (UIStateManager.Instance.isUIOpen) return;

        Move();
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        if (Input.GetKeyDown(KeyCode.W)&& isGrounded)
        {
            Jump();
        }
    }

    void Move()
    {
        float move = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(move * playerData.moveSpeed, rb.velocity.y);

        if (move > 0 && !facingRight)
        {
            Flip();
        }
        else if (move < 0 && facingRight)
        {
            Flip();
        }
    }

    void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, playerData.jumpForce);
    }

    void Flip()
    {
        facingRight = !facingRight;

        // X 스케일 반전
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }
}
