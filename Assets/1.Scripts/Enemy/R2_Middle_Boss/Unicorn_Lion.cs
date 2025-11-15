using System.Collections;
using UnityEngine;

public class Unicorn_Lion : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 3f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    public bool isGrounded;

    [Header("Facing")]
    public SpriteRenderer sr;
    public bool FRD = true; // 스프라이트 기본 방향
    private bool FR = true; // 현재 바라보는 방향

    [Header("Attack Pattern System")]
    public Transform firePoint;
    public GameObject FeetPrefab;
    public GameObject WaterBreathPrefab;
    public float attackRangeTiles = 7f;

    public float attackCooldown = 2f;
    private float nextAttackTime = 0f;
    private bool isPatternPlaying = false;

    private Rigidbody2D rb;
    private Transform player;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }
    private void Update()
    {
        // 패턴 중이면 이동 불가능, 다른 패턴도 실행 불가능
        if (isPatternPlaying) return;

        if (player != null)
            TryPattern();
    }

    private void FixedUpdate()
    {
        isGrounded = CheckGrounded();

        if (!isPatternPlaying)   //패턴 중에는 추적 금지
        {
            if (player != null)
                ChasePlayer();
        }

        ApplyFlip();
    }

    private bool CheckGrounded()
    {
        Vector2 pos;

        if (groundCheck != null)
            pos = groundCheck.position;
        else
        {
            var col = GetComponent<Collider2D>();
            if (col != null)
                pos = new Vector2(col.bounds.center.x, col.bounds.min.y - 0.02f);
            else
                pos = new Vector2(transform.position.x, transform.position.y - 0.3f);
        }

        return Physics2D.OverlapCircle(pos, groundCheckRadius, groundLayer);
    }

    private void ChasePlayer()
    {
        if (!isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x * 0.95f, rb.velocity.y);
            return;
        }

        float dirX = Mathf.Sign(player.position.x - transform.position.x);
        rb.velocity = new Vector2(dirX * moveSpeed, rb.velocity.y);

        FR = dirX > 0;
    }


    private void TryPattern()
    {
        if (player == null) return;
        if (isPatternPlaying) return;
        if (Time.time < nextAttackTime) return;

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist > attackRangeTiles) return;

        nextAttackTime = Time.time + attackCooldown;

        int d = Random.Range(1, 101);
        Debug.Log("랜덤 패턴 값 : " + d);

        if (d <= 25)
            StartCoroutine(Pattern1_FeetProjectile());
        else if (d <= 50)
            StartCoroutine(Pattern2_Breath());
        else if (d <= 75)
            StartCoroutine(Pattern3_Delay());
        else
            StartCoroutine(Pattern4_DoubleShot());
    }


    private IEnumerator Pattern1_FeetProjectile()
    {
        isPatternPlaying = true;

        Debug.Log("패턴 1: 평타");

        ShootProjectile();

        yield return new WaitForSeconds(0.3f);
        isPatternPlaying = false;
    }


    private IEnumerator Pattern2_Breath()
    {
        isPatternPlaying = true;

        Debug.Log("패턴 2 : 브레스");

        rb.velocity = Vector2.zero; // 멈추기
        float breathTime = 1.5f;
        float shotInterval = 0.2f;
        float timer = 0f;

        while (timer < breathTime)
        {
            Breath();
            yield return new WaitForSeconds(shotInterval);
            timer += shotInterval;
        }

        yield return new WaitForSeconds(0.3f);
        isPatternPlaying = false;
    }


    private IEnumerator Pattern3_Delay()
    {
        isPatternPlaying = true;
        Debug.Log("패턴: 휴식");

        yield return new WaitForSeconds(0.8f);

        isPatternPlaying = false;
    }

    private IEnumerator Pattern4_DoubleShot()
    {
        isPatternPlaying = true;
        Debug.Log("패턴: 더블 샷");

        ShootProjectile();
        yield return new WaitForSeconds(0.3f);
        ShootProjectile();

        yield return new WaitForSeconds(0.2f);
        isPatternPlaying = false;
    }

    private void ShootProjectile()  //앞발 공격
    {
        if (FeetPrefab == null || firePoint == null || player == null) return;

        GameObject proj = Instantiate(FeetPrefab, firePoint.position, Quaternion.identity);
        Vector2 dir = (player.position - firePoint.position).normalized;

        Rigidbody2D prb = proj.GetComponent<Rigidbody2D>();
        if (prb != null)
            prb.velocity = dir * 10f;
    }
    private void Breath()  //브레스 공격
    {
        if (FeetPrefab == null || firePoint == null || player == null) return;

        GameObject proj = Instantiate(WaterBreathPrefab, firePoint.position, Quaternion.identity);
        Vector2 dir = (player.position - firePoint.position).normalized;

        Rigidbody2D prb = proj.GetComponent<Rigidbody2D>();
        if (prb != null)
            prb.velocity = dir * 10f;
    }

    private void ApplyFlip()
    {
        if (sr == null) return;

        bool flip = FRD ? !FR : FR;
        sr.flipX = flip;

        if (firePoint != null)
        {
            Vector3 lp = firePoint.localPosition;
            lp.x = Mathf.Abs(lp.x) * (FR ? 1 : -1);
            firePoint.localPosition = lp;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        if (groundCheck != null)
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        else
            Gizmos.DrawWireSphere(transform.position + Vector3.down * 0.3f, groundCheckRadius);
    }
}
