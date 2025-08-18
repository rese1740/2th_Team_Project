using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BoundaryEnemyL : MonoBehaviour
{
    [Header("Patrol")]
    public float patrolSpeed = 3f; // 순찰 속도

    [Header("Detection")]
    public float detectRangeTiles = 5f;
    public float tileSize = 1f;

    [Header("Alert")]
    public GameObject alertIconPrefab;
    public Vector3 alertOffset = new Vector3(0, 1.5f, 0);
    public float alertDuration = 0.3f;
    public float bounceHeight = 0.3f;
    public float bounceSpeed = 6f;

    [Header("Ranged Attack")]
    public Transform firePoint;          // 발사 위치
    public GameObject projectilePrefab;  // 발사체 프리팹
    public float projectileSpeed = 8f;   // 발사 속도
    public float attackInterval = 0.8f;  // 발사 간격
    public bool attackOnlyHorizontal = false; // true면 좌우만 조준

    [Header("Visuals")]
    public SpriteRenderer spriteRenderer;
    public Transform gfxRoot;

    private Rigidbody2D rb;
    private bool movingRight = true;
    private Transform player;

    private enum State { Patrol, Alert, Attack }
    private State state = State.Patrol;

    private bool isAlerting = false;
    private bool attackingLoopRunning = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (gfxRoot == null) gfxRoot = spriteRenderer != null ? spriteRenderer.transform : transform;
        if (firePoint == null) firePoint = transform; // 안전장치
    }

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void FixedUpdate()
    {
        bool playerInRange = IsPlayerInRange();

        switch (state)
        {
            case State.Patrol:
                if (playerInRange && !isAlerting)
                    StartCoroutine(EnterAlert());
                break;
            case State.Alert:
                // 코루틴에서 Attack으로 전이
                break;
            case State.Attack:
                if (!playerInRange)
                {
                    state = State.Patrol;
                    attackingLoopRunning = false;
                }
                break;
        }

        MoveByState();
        ApplyFlip();
    }

    private bool IsPlayerInRange()
    {
        if (player == null) return false;
        float r = detectRangeTiles * tileSize;
        return Vector2.Distance(transform.position, player.position) <= r;
    }

    private IEnumerator EnterAlert()
    {
        isAlerting = true;
        state = State.Alert;

        rb.velocity = Vector2.zero;

        // 알림 아이콘 생성
        if (alertIconPrefab != null)
        {
            var icon = Instantiate(alertIconPrefab, gfxRoot.position + alertOffset, Quaternion.identity, gfxRoot);
            Destroy(icon, alertDuration);
        }

        // 튕김 연출
        yield return StartCoroutine(BounceAnimation());

        // alertDuration 대기
        yield return new WaitForSeconds(alertDuration);

        // 여전히 범위 안에 있으면 공격 시작
        state = IsPlayerInRange() ? State.Attack : State.Patrol;
        isAlerting = false;
    }

    private IEnumerator BounceAnimation()
    {
        float half = 0.5f / bounceSpeed;
        float elapsed = 0f;
        Vector3 start = gfxRoot.localPosition;
        Vector3 peak = start + Vector3.up * bounceHeight;

        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / half);
            gfxRoot.localPosition = Vector3.Lerp(start, peak, t);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / half);
            gfxRoot.localPosition = Vector3.Lerp(peak, start, t);
            yield return null;
        }

        gfxRoot.localPosition = start;
    }

    private void MoveByState()
    {
        switch (state)
        {
            case State.Patrol:
                rb.velocity = new Vector2((movingRight ? 1f : -1f) * patrolSpeed, rb.velocity.y);
                break;
            case State.Alert:
                rb.velocity = Vector2.zero;
                break;
            case State.Attack:
                rb.velocity = Vector2.zero;
                if (!attackingLoopRunning)
                {
                    attackingLoopRunning = true;
                    StartCoroutine(AttackLoop());
                }
                break;
        }
    }

    private IEnumerator AttackLoop()
    {
        while (state == State.Attack)
        {
            FireOnce();
            yield return new WaitForSeconds(attackInterval);
        }
        attackingLoopRunning = false;
    }

    private void FireOnce()
    {
        if (projectilePrefab == null || player == null) return;

        // 조준 방향
        Vector2 dir = (player.position - firePoint.position);
        if (attackOnlyHorizontal)
        {
            float sx = Mathf.Sign(dir.x);
            if (sx == 0) sx = movingRight ? 1f : -1f;
            dir = new Vector2(sx, 0f);
        }
        dir = dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector2.right;

        movingRight = dir.x >= 0f;

        // 발사체 생성
        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        // BELBullet 있으면 초기화
        var bullet = proj.GetComponent<BELBullet>();
        if (bullet != null)
        {
            bullet.Init(dir, projectileSpeed);
        }
        else
        {
            // 예비: Rigidbody2D만 있을 때
            var prb = proj.GetComponent<Rigidbody2D>();
            if (prb != null) prb.velocity = dir * projectileSpeed;

            Destroy(proj, 5f); // 안전하게 수명 보장
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (state == State.Patrol && collision.CompareTag("Boundary"))
        {
            movingRight = !movingRight;
        }
    }

    private void ApplyFlip()
    {
        if (spriteRenderer == null) return;

        // 스프라이트 뒤집기
        spriteRenderer.flipX = !movingRight;

        // firePoint도 같이 반전
        if (firePoint != null)
        {
            // 위치 반전
            var lp = firePoint.localPosition;
            lp.x = Mathf.Abs(lp.x) * (movingRight ? 1 : -1);
            firePoint.localPosition = lp;

            // 시선 각도 정렬: 오른쪽 0°, 왼쪽 180°(Y축)
            firePoint.localRotation = movingRight
                ? Quaternion.Euler(0, 0, 0)
                : Quaternion.Euler(0, 180f, 0);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        float r = detectRangeTiles * tileSize;
        Gizmos.DrawWireSphere(transform.position, r);

        if (firePoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(firePoint.position, 0.05f);
        }
    }
}
