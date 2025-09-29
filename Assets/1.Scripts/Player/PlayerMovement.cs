using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Settings")]
    public bool isGrounded;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    private bool facingRight = true;

    [Header("Components")]
    public PlayerSO playerData;
    Animator animator;
    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;

    private void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (UIStateManager.Instance.isUIOpen) return;

        Move();
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        if (Input.GetKeyDown(KeyCode.W) && isGrounded)
        {
            Jump();
        }
    }

    void Move()
    {
        float move = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(move * playerData.moveSpeed, rb.velocity.y);

        bool isWalking = move != 0;
        animator.SetBool("isMove", isWalking);

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
        animator.SetTrigger("jump");
        rb.velocity = new Vector2(rb.velocity.x, playerData.jumpForce);
    }

    void Flip()
    {
        facingRight = !facingRight;

        // X ������ ����
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }
}
