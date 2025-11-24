using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BoundaryEnemy : MonoBehaviour
{
    public enum State { Patrol, Alert, Chase, Attack }

    [Header("바운더리 적 So불러오기")]
    public EnemySO config;

    [Header("씬 참조")]
    public Transform firePoint;             // 발사 지점(총구)
    public SpriteRenderer spriteRenderer;   // 적 스프라이트 렌더러
    public Transform gfxRoot;

    [Header("지면 감지")]
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.2f; // 바닥 감지 거리
    public float airGravityScale = 3f;       // 공중일 때 중력
    public float groundGravityScale = 0f;    // Ground 위일 때 중력

    [Header("피격/넉백 제어")]
    [Tooltip("넉백 중 이동 AI를 일시 정지할지")]
    public bool pauseAIWhileKnockback = true;

    private bool isKnockback = false;
    private float knockbackEndTime = 0f;

    private Rigidbody2D rb;                 // 물리 바디
    private Transform player;               // 플레이어 Transform

    private bool movingRight = true;        // 현재 바라보는 방향(true=우측)
    private bool isAlerting = false;        // 경고 연출 중 여부(중복 방지)
    private bool hasAggro = false;          // 플레이어에게 어그로가 붙었는지

    private Coroutine attackRoutine;        // 공격 루프 코루틴 핸들
    private float nextShotTime = 0f;        // 쿨다운 관리

    private State state = State.Patrol;     // 현재 상태
    private bool isOnGround = false;        // Ground 감지 결과

    private Animator anim;


    void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        // 참조 자동 보정
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (gfxRoot == null)
            gfxRoot = spriteRenderer != null ? spriteRenderer.transform : transform;

        if (firePoint == null)
            firePoint = transform;
    }

    void Start()
    {

        // 플레이어 참조
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void OnDisable()
    {
        // 비활성화 시 안전 정리
        StopAttackLoop();
        rb.velocity = Vector2.zero;
        hasAggro = false;
    }

    void FixedUpdate()
    {
        // 넉백 타이머 갱신
        if (isKnockback && Time.time >= knockbackEndTime)
            isKnockback = false;

        bool inRange = IsPlayerInRange();

        if (!isKnockback) // 넉백 아닐 때만 상태 머신 전이
        {
            switch (state)
            {
                case State.Patrol:
                    if (!hasAggro && inRange && !isAlerting)
                        StartCoroutine(EnterAlert());
                    break;
                case State.Alert:
                    break;
                case State.Chase:
                    if (inRange) state = State.Attack;
                    else if (!config.persistAggro)
                    {
                        state = State.Patrol;
                        hasAggro = false;
                    }
                    break;
                case State.Attack:
                    if (!inRange)
                    {
                        state = State.Chase;
                        StopAttackLoop();
                    }
                    break;
            }

            MoveByState();  //  넉백 아닐 때만 AI가 속도 제어
        }
        else
        {
            // 넉백 중: AI 속도 세팅 금지
            StopAttackLoop(); // 공격 루프도 잠시 정지 (원치 않으면 제거)

            // 보는 방향은 현재 속도 기준으로만 업데이트(선택)
            if (rb.velocity.x != 0f)
                movingRight = rb.velocity.x > 0f;
        }

        ApplyFlip();
        CheckGround();
    }

    // 플레이어가 탐지 범위 안에 있는지 확인
    private bool IsPlayerInRange()
    {
        if (player == null || config == null) return false;
        float r = config.detectRangeTiles * config.tileSize;
        return Vector2.Distance(transform.position, player.position) <= r;
    }

    // 경고 상태 진입(느낌표 아이콘 + 깡총 애니메이션 + 대기 후 추격으로 전이)
    private IEnumerator EnterAlert()
    {
        if (config == null) yield break;

        isAlerting = true;
        state = State.Alert;
        rb.velocity = Vector2.zero;

        if (config.alertIconPrefab != null)
        {
            var icon = Instantiate(config.alertIconPrefab, gfxRoot.position + config.alertOffset, Quaternion.identity, gfxRoot);
            Destroy(icon, config.alertDuration);
        }

        // 경고 연출(깡총)
        yield return StartCoroutine(BounceAnimation());
        yield return new WaitForSeconds(config.alertDuration);


        // 추격 시작
        hasAggro = true;
        state = State.Chase;
        isAlerting = false;
    }

    // 바운스 애니메이션
    private IEnumerator BounceAnimation()
    {
        if (config == null) yield break;

        float half = 0.5f / Mathf.Max(0.0001f, config.bounceSpeed);
        float elapsed = 0f;

        Vector3 start = gfxRoot.localPosition;
        Vector3 peak = start + Vector3.up * config.bounceHeight;

        // 위로
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / half);
            gfxRoot.localPosition = Vector3.Lerp(start, peak, t);
            yield return null;
        }

        // 아래로
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

    //상태별 이동/속도/공격 루프 제어
    private void MoveByState()
    {
        if (config == null) return;

        switch (state)
        {
            case State.Patrol:
                // 좌우 순찰
                rb.velocity = new Vector2((movingRight ? 1f : -1f) * config.patrolSpeed, rb.velocity.y);
                StopAttackLoop();
                break;

            case State.Alert:
                // 경고 중에는 정지
                rb.velocity = Vector2.zero;
                StopAttackLoop();
                break;

            case State.Chase:
                // 추격 상태에서는 공격 루프 중단, 옵션에 따라 플레이어 쪽으로 x 이동
                StopAttackLoop();
                if (config.chaseWhileAggro && hasAggro && player != null)
                {
                    float dirX = Mathf.Sign(player.position.x - transform.position.x);
                    rb.velocity = new Vector2(dirX * config.chaseSpeed, rb.velocity.y);
                    movingRight = dirX >= 0f;
                }
                else
                {
                    rb.velocity = new Vector2(0f, rb.velocity.y);
                }
                break;

            case State.Attack:
                // 공격 상태: 옵션에 따라 이동을 유지하며 사격하거나 제자리 사격
                if (config.chaseWhileAggro && hasAggro && player != null)
                {
                    float dirX = Mathf.Sign(player.position.x - transform.position.x);
                    rb.velocity = new Vector2(dirX * config.chaseSpeed, rb.velocity.y);
                    movingRight = dirX >= 0f;
                }
                else
                {
                    rb.velocity = new Vector2(0f, rb.velocity.y);
                }
                StartAttackLoop(); // 단일 루프 보장
                break;
        }
    }

    // 공격 루프 시작
    private void StartAttackLoop()
    {
        if (attackRoutine == null)
            attackRoutine = StartCoroutine(AttackLoop());
    }


    // 공격 루프 중지
    private void StopAttackLoop()
    {
        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }
    }

    // 공격 루프: 쿨다운을 고려하여 반복 발사
    private IEnumerator AttackLoop()
    {
        anim.SetTrigger("Attack");
        while (state == State.Attack)
        {
            if (IsPlayerInRange() && Time.time >= nextShotTime)
            {
                FireOnce();
                nextShotTime = Time.time + Mathf.Max(0f, config.attackInterval);
            }
            yield return null;  // 매 프레임 체크
        }
        attackRoutine = null;
    }


    // 1회 발사 처리
    private void FireOnce()
    {
        if (config == null || config.projectilePrefab == null || player == null)
            return;

        Vector2 dir = (player.position - firePoint.position);

        // 수평 전용 발사 모드 
        if (config.attackOnlyHorizontal)
        {
            float sx = Mathf.Sign(dir.x);
            if (sx == 0) sx = movingRight ? 1f : -1f;
            dir = new Vector2(sx, 0f);
        }

        dir = dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector2.right;
        movingRight = dir.x >= 0f;

        // 투사체 생성
        GameObject proj = Instantiate(config.projectilePrefab, firePoint.position, Quaternion.identity);

        var bullet = proj.GetComponent<BELBullet>();
        if (bullet != null)
            bullet.Init(dir, config.projectileSpeed);
        else
        {
            var prb = proj.GetComponent<Rigidbody2D>();
            if (prb != null) prb.velocity = dir * config.projectileSpeed;
            Destroy(proj, 5f);
        }
    }

    // 경계 트리거에 닿으면 순찰 방향 반전
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (state == State.Patrol && collision.CompareTag("Boundary"))
            movingRight = !movingRight;
    }

    // 바라보는 방향에 맞게 스프라이트 및 발사포인트 반전
    private void ApplyFlip()
    {
        if (spriteRenderer == null) return;

        spriteRenderer.flipX = !movingRight;

        if (firePoint != null)
        {
            var lp = firePoint.localPosition;
            lp.x = Mathf.Abs(lp.x) * (movingRight ? 1 : -1);
            firePoint.localPosition = lp;

            firePoint.localRotation = movingRight
                ? Quaternion.Euler(0, 0, 0)
                : Quaternion.Euler(0, 180f, 0);
        }
    }

    // Ground 지면 감지
    private void CheckGround()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);
        bool hitGround = hit.collider != null;

        if (hitGround)
        {
            //  넉백 중에는 상승 중(y>0)일 때는 중력/속도 클램프 금지 → 자연스러운 호 상승 유지
            if (isKnockback && rb.velocity.y > 0f)
            {
                rb.gravityScale = airGravityScale;  // 올라갈 땐 중력 유지
                return;
            }

            // 하강 중/정지일 때만 "바닥처럼" 붙이기
            isOnGround = true;
            rb.gravityScale = groundGravityScale;
            rb.velocity = new Vector2(rb.velocity.x, 0f);
        }
        else
        {
            isOnGround = false;
            rb.gravityScale = airGravityScale;
        }
    }

    public void EnterKnockback(float duration)
    {
        isKnockback = true;
        knockbackEndTime = Time.time + duration;
        StopAttackLoop();
    }

    private void OnDrawGizmosSelected()
    {
        // 탐지 범위 표시
        if (config != null)
        {
            Gizmos.color = Color.yellow;
            float r = config.detectRangeTiles * config.tileSize;
            Gizmos.DrawWireSphere(transform.position, r);
        }

        // 발사 지점 표시
        if (firePoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(firePoint.position, 0.05f);
        }

        // 바닥 감지 레이 시각화
        Gizmos.color = isOnGround ? Color.green : Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
    }
}
