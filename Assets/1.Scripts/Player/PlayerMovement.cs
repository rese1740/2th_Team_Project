using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Settings")]
    public bool isGrounded;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public bool facingRight = true;

    public float dashSpeed = 12f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 0.5f;
    public bool isDashing = false;
    public bool canDash = true;
    public bool useGhostEffect = false;

    [Header("Components")]
    public PlayerSO playerData;
    Animator animator;
    Rigidbody2D rb;
    PlayerAttack pa;
    SpriteRenderer spriteRenderer;
    PlayerGhost pg;

    //-----추가-----
    [Header("Status / Debuff")]
    public bool isStunned = false;      // 기절 상태
    private float stunEndTime = 0f;

    public float slowAmount = 0f;       // 감소된 이동속도 양
    private float slowEndTime = 0f;     // 슬로우 끝나는 시간
    //-----추가-----

    private void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        pa = GetComponent<PlayerAttack>();
        pg = GetComponent<PlayerGhost>();
    }

    private void Update()
    {
        //-----추가-----
        // 스턴 시간 체크
        if (isStunned)
        {
            if (Time.time >= stunEndTime)
            {
                // 스턴 해제
                isStunned = false;
            }
            else
            {
                // 스턴 중에는 움직임/입력 전부 막기
                rb.velocity = Vector2.zero;
                animator.SetBool("isMove", false);
                animator.SetBool("isDash", false);
                animator.SetBool("isFalling", false);
                return;
            }
        }
        //-----추가-----

        if (pa.isAction || UIStateManager.Instance.isUIOpen)
        {
            rb.velocity = Vector2.zero;
            animator.SetBool("isMove", false);
            return;
        }

        if (isDashing)
            return;

        Move();
        Fall();
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        if (Input.GetKeyDown(KeyCode.W) && isGrounded)
        {
            Jump();
        }
        //-----추가-----
        // 슬로우 해제 체크
        if (slowAmount > 0f && Time.time >= slowEndTime)
        {
            slowAmount = 0f;
        }
        //-----추가-----

    }


    //-----추가-----
    public void ApplySpeedDebuff(float amount, float duration)
    {
        // 슬로우 중복 적용 시 더 큰 지속시간을 유지
        slowAmount += amount;
        slowEndTime = Mathf.Max(slowEndTime, Time.time + duration);
    }

    public void Stun(float duration)
    {
        if (duration <= 0f) return;

        isStunned = true;
        stunEndTime = Mathf.Max(stunEndTime, Time.time + duration);

        //멈추기
        rb.velocity = Vector2.zero;
        animator.SetBool("isMove", false);
        animator.SetBool("isDash", false);
    }
    //-----추가-----

    public void StartDash(float power, float duraction, bool effect)
    {
        if (effect)
            pg.StartGhost();

        dashDuration = duraction;
        dashSpeed = power;
        StartCoroutine(Dash());
        return;
    }

    IEnumerator Dash()
    {
        isDashing = true;
        canDash = false;

        animator.SetBool("isDash", true);

        // 바라보는 방향으로 대시
        float dashDirection = facingRight ? 1f : -1f;

        rb.gravityScale = 0; // 대시 중 중력 무시
        rb.velocity = new Vector2(dashDirection * dashSpeed, 0);

        yield return new WaitForSeconds(dashDuration);
        pg.StopGhost();

        rb.gravityScale = 3; // 다시 기본 중력 설정 (너의 기본값에 맞춰 변경)
        isDashing = false;
        animator.SetBool("isDash", false);
        canDash = true;
    }

    void Move()
    {
        float move = Input.GetAxisRaw("Horizontal");

        //-----추가-----
        // 슬로우 적용: 기본 속도 - slowAmount (0 이하로는 내려가지 않도록)
        float currentSpeed = Mathf.Max(0f, playerData.moveSpeed - slowAmount);
        //-----추가-----

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
    void Fall()
    {
        if (!isGrounded && rb.velocity.y < -0.1f)
        {
            animator.SetBool("isFalling", true);
        }
        else
        {
            animator.SetBool("isFalling", false);
        }
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
