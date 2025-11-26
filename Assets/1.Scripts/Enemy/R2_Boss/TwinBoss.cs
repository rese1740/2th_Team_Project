using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class TwinBoss : MonoBehaviour
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
    [Tooltip("스프라이트 기본 방향 (true = 오른쪽을 향함)")]
    public bool FRD = true;
    private bool FR = true; // 현재 바라보는 방향

    [Header("Attack / Pattern System")]
    public Transform firePoint;
    public float attackRangeTiles = 7f;
    public float attackCooldown = 1.5f;
    private float nextAttackTime = 0f;

    [Header("Twin References")]
    public TwinBoss partner;
    public BossTwinHp myHealth;
    public BossTwinHp partnerHealth;
    private bool partnerDead = false;
    private bool InSoloPhase => partnerDead; // 한 명만 남았는지

    [Header("Twin Control")]
    [Tooltip("트윈 패턴의 리더인지 여부 (한 마리만 true)")]
    public bool isLeader = false;
    [SerializeField] private bool isPatternPlaying = false;
    public bool IsPatternPlaying => isPatternPlaying;

    // ===================== 패턴1: 투사체 =====================
    [Header("Pattern1: Projectile")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 10f;

    // ===================== 패턴2: 점프 =====================
    [Header("Pattern2: Jump Attack")]
    public float jumpForceY = 8f;

    [Tooltip("점프 중 스쳤을 때 데미지")]
    public int jumpTouchDamage = 10;
    [Tooltip("착지 직후 맞았을 때 데미지")]
    public int landingHitDamage = 15;
    [Tooltip("착지 직후 맞았을 때 플레이어 이동속도 감소량")]
    public float landingSlowAmount = 2f;

    [Tooltip("착지 후 플레이어 기준 바깥쪽으로 이동 속도")]
    public float jumpOutwardMoveSpeed = 4f;
    [Tooltip("착지 후 바깥쪽 이동 유지 시간")]
    public float jumpOutwardMoveDuration = 0.7f;

    private bool isJumpAttackActive = false;
    private bool landingHitWindow = false;

    // ===================== 패턴3: 레이저 (음파) =====================
    [Header("Pattern3: Laser / Sound Wave")]
    public GameObject laserLinePrefab;
    public LayerMask playerLayer;

    [Tooltip("Twin 페이즈 텔레그래프 시간")]
    public float twinTelegraphTime = 1.5f;
    [Tooltip("Twin 레이저 유지 시간")]
    public float twinLaserDuration = 3f;
    [Tooltip("Twin 레이저 1틱 데미지")]
    public float twinLaserDamage = 25f;

    [Tooltip("Solo 페이즈 텔레그래프 시간")]
    public float soloTelegraphTime = 1.0f;
    [Tooltip("Solo 레이저 유지 시간")]
    public float soloLaserDuration = 3f;
    [Tooltip("Solo 레이저 1틱 데미지")]
    public float soloLaserDamage = 30f;

    [Header("Laser Move Settings")]
    public float laserSideOffset = 10f;

    [Tooltip("레이저 기준 높이 offset (firePoint 기준)")]
    public float laserYOffset = 0f;
    [Tooltip("Solo 두 줄 레이저 사이 간격")]
    public float soloLaserLineGap = 0.5f;

    // ===================== 패턴4: 돌진 (붙으려 이동) =====================
    [Header("Pattern4: Charge To Player")]
    public float chargeSpeed = 10f;
    public float chargeDuration = 1f;        // 최대 1초 동안 돌진

    [Tooltip("Twin에서 부딪힌 후 바깥으로 느리게 이동 속도")]
    public float outwardSlowSpeed = 2f;
    [Tooltip("Twin에서 바깥으로 이동 + 가만히 있는 시간")]
    public float outwardSlowDuration = 2f;

    [Tooltip("Twin 돌진 중 맞았을 때 첫 데미지")]
    public int twinChargeTouchDamageFirst = 10;
    [Tooltip("Twin 돌진에서 추가 데미지(스턴 포함)")]
    public int twinChargeExtraDamageOnStun = 20;
    [Tooltip("Twin 돌진 맞으면 기절 시간")]
    public float twinChargeStunDuration = 1.5f;

    [Tooltip("Solo 첫 돌진에서 맞았을 때 데미지")]
    public int soloChargeFirstHitDamage = 15;
    [Tooltip("Solo 두 번째 돌진에서 맞았을 때 데미지")]
    public int soloChargeSecondHitDamage = 20;

    private bool isChargeAttackActive = false;
    private int currentChargeDashIndex = 0; // Solo: 1번째/2번째 돌진 구분
    private bool chargeHitThisDash = false; // Solo: 이번 돌진 중 맞췄는지

    // ===================== Solo 회복 패턴 =====================
    [Header("Solo Heal Phase")]
    public GameObject healCirclePrefab;        // 보스 주변 원 이펙트
    public float soloHealDuration = 4f;        // 4초 동안
    public float soloHealInterval = 0.5f;      // 0.5초마다
    public float soloHealAmountPerTick = 10f;  // 틱당 +10
    private GameObject healCircleInstance;
    private bool soloHealTriggered = false;    // 한 번만 자동 발동

    // ===================== 내부용 =====================
    private Rigidbody2D rb;
    private Transform player;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (!sr) sr = GetComponentInChildren<SpriteRenderer>();
        if (!myHealth) myHealth = GetComponent<BossTwinHp>();
    }

    private void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        if (partnerHealth != null)
        {
            partnerHealth.onDeath.AddListener(OnPartnerDeath);
        }
    }

    private void Update()
    {
        if (isPatternPlaying) return;
        if (player != null)
            TryPattern();
    }

    private void FixedUpdate()
    {
        isGrounded = CheckGrounded();

        if (!isPatternPlaying)
        {
            if (player != null)
                ChasePlayer();
        }

        ApplyFlip();
    }

    // ===================== 공통: Ground / Move / Flip =====================

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

    private void ApplyFlip()
    {
        if (sr == null) return;

        bool flip = FRD ? !FR : FR;
        sr.flipX = flip;

        // 패턴 실행 중에는 firePoint 위치 고정(레이저/투사체 방향 꼬임 방지)
        if (!isPatternPlaying)
        {
            if (firePoint != null)
            {
                Vector3 lp = firePoint.localPosition;
                lp.x = Mathf.Abs(lp.x) * (FR ? 1 : -1);
                firePoint.localPosition = lp;
            }
        }
    }

    // ===================== 패턴 선택 (리더 기준 동기화) =====================

    private void TryPattern()
    {
        if (player == null) return;
        if (Time.time < nextAttackTime) return;

        bool twinPhase = (!InSoloPhase && partnerHealth != null && !partnerDead);

        if (twinPhase)
        {
            // 리더만 패턴 뽑기
            if (!isLeader) return;

            // 둘 중 아무나 패턴 중이면 새 패턴 X
            if (isPatternPlaying || (partner != null && partner.IsPatternPlaying))
                return;

            float dist = Vector2.Distance(transform.position, player.position);
            if (dist > attackRangeTiles) return;

            nextAttackTime = Time.time + attackCooldown;

            int d = Random.Range(1, 101);
            Debug.Log($"[LEADER {name}] 트윈 랜덤 패턴 값 : {d}");

            if (d <= 25)
            {
                PlayPattern1();
                if (partner != null) partner.PlayPattern1();
            }
            else if (d <= 50)
            {
                PlayPattern2();
                if (partner != null) partner.PlayPattern2();
            }
            else if (d <= 75)
            {
                PlayPattern3();
                if (partner != null) partner.PlayPattern3();
            }
            else
            {
                PlayPattern4();
                if (partner != null) partner.PlayPattern4();
            }
        }
        else
        {
            // Solo 페이즈: 남은 보스 혼자 패턴
            if (isPatternPlaying) return;

            float dist = Vector2.Distance(transform.position, player.position);
            if (dist > attackRangeTiles) return;

            nextAttackTime = Time.time + attackCooldown;

            int d = Random.Range(1, 101);
            Debug.Log($"[SOLO {name}] 랜덤 패턴 값 : {d}");

            if (d <= 25)
                PlayPattern1();
            else if (d <= 50)
                PlayPattern2();
            else if (d <= 75)
                PlayPattern3();
            else
                PlayPattern4();
        }
    }

    // ===== Twin / Solo 공통으로 호출하는 패턴 시작 함수들 =====
    public void PlayPattern1()
    {
        if (!isPatternPlaying)
            StartCoroutine(Pattern1_Projectile());
    }

    public void PlayPattern2()
    {
        if (!isPatternPlaying)
            StartCoroutine(Pattern2_JumpToPlayer());
    }

    public void PlayPattern3()
    {
        if (!isPatternPlaying)
            StartCoroutine(Pattern3_Laser());
    }

    public void PlayPattern4()
    {
        if (!isPatternPlaying)
            StartCoroutine(Pattern4_ChargeToPlayer());
    }

    // ===================== 패턴1: 투사체 =====================

    private IEnumerator Pattern1_Projectile()
    {
        isPatternPlaying = true;
        Debug.Log($"[{name}] 패턴1: 투사체 발사");

        rb.velocity = Vector2.zero;

        if (projectilePrefab != null && firePoint != null && player != null)
        {
            GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            Rigidbody2D prb = proj.GetComponent<Rigidbody2D>();

            if (prb != null)
            {
                Vector2 dir = (player.position - firePoint.position).normalized;
                prb.velocity = dir * projectileSpeed;
            }
        }

        yield return new WaitForSeconds(0.3f);
        isPatternPlaying = false;
    }

    // ===================== 패턴2: 점프 =====================

    private IEnumerator Pattern2_JumpToPlayer()
    {
        isPatternPlaying = true;
        isJumpAttackActive = true;
        landingHitWindow = false;

        Debug.Log($"[{name}] 패턴2: 점프 공격 시작");

        rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(0.2f);

        // 플레이어 위치로 정확히 착지하는 포물선 점프
        if (player != null)
        {
            float Vy = jumpForceY;
            float gravity = Mathf.Abs(Physics2D.gravity.y * rb.gravityScale);
            float t = (2f * Vy) / gravity;

            float distX = player.position.x - transform.position.x;
            float Vx = distX / t;

            rb.velocity = Vector2.zero;
            rb.AddForce(new Vector2(Vx, Vy), ForceMode2D.Impulse);

            FR = Vx > 0;
        }

        // 공중 → 착지까지 기다림 (타임아웃 안전장치)
        bool hasLeftGround = false;
        float timeout = 5f;
        while (timeout > 0f)
        {
            timeout -= Time.deltaTime;

            if (!isGrounded)
            {
                hasLeftGround = true;
            }
            else if (hasLeftGround && isGrounded)
            {
                break;
            }

            yield return null;
        }

        // 착지 직후 히트 윈도우 열기
        landingHitWindow = true;
        Debug.Log($"[{name}] 점프 착지! 히트 윈도우 열림");
        yield return new WaitForSeconds(0.1f);
        landingHitWindow = false;

        // 착지 후 플레이어 기준 바깥쪽으로 이동
        if (player != null)
        {
            float outwardDirX = Mathf.Sign(transform.position.x - player.position.x); // 플레이어 기준 바깥
            float t2 = 0f;
            while (t2 < jumpOutwardMoveDuration)
            {
                rb.velocity = new Vector2(outwardDirX * jumpOutwardMoveSpeed, rb.velocity.y);
                FR = outwardDirX > 0;
                t2 += Time.deltaTime;
                yield return null;
            }
        }

        rb.velocity = new Vector2(0f, rb.velocity.y);

        isJumpAttackActive = false;
        Debug.Log($"[{name}] 패턴2: 점프 패턴 종료");

        yield return new WaitForSeconds(0.2f);
        isPatternPlaying = false;
    }

    // ===================== 패턴3: 레이저(음파) =====================

    private void SpawnLaserLine(Vector3 worldPos, float telegraphTime, float damage, float duration)
    {
        if (laserLinePrefab == null) return;

        GameObject obj = Instantiate(laserLinePrefab, worldPos, Quaternion.identity);
        BossLaser laser = obj.GetComponent<BossLaser>();
        if (laser != null)
        {
            laser.Setup(telegraphTime, damage, 1f, duration, playerLayer);
        }
    }

    private IEnumerator Pattern3_Laser()
    {
        isPatternPlaying = true;
        rb.velocity = Vector2.zero;

        bool twinPhase = (!InSoloPhase && partnerHealth != null && !partnerDead);

        if (twinPhase)
        {
            Debug.Log($"[{name}] 패턴3: Twin 레이저");

            // 1) Twin 보스 위치에 따라 카메라 양쪽 끝으로 이동 
            bool amILeftBoss = transform.position.x < partner.transform.position.x;

            if (amILeftBoss)
                yield return StartCoroutine(MoveToPlayerSide(false, laserSideOffset)); // 왼쪽 끝
            else
                yield return StartCoroutine(MoveToPlayerSide(true, laserSideOffset));  // 오른쪽 끝

            // 2) 카메라 끝에 도착 후 레이저 발사 
            Vector3 basePos = firePoint != null ? firePoint.position : transform.position;
            basePos.y += laserYOffset;

            SpawnLaserLine(basePos, twinTelegraphTime, twinLaserDamage, twinLaserDuration);

            float totalWait = twinTelegraphTime + twinLaserDuration + 2f;
            float t = 0f;
            while (t < totalWait)
            {
                rb.velocity = Vector2.zero;
                t += Time.deltaTime;
                yield return null;
            }
        }
        else
        {
            Debug.Log($"[{name}] 패턴3: Solo 레이저 (2줄)");

            Vector3 basePos = firePoint != null ? firePoint.position : transform.position;
            basePos.y += laserYOffset;

            Vector3 line1Pos = basePos + Vector3.up * soloLaserLineGap;
            Vector3 line2Pos = basePos + Vector3.down * soloLaserLineGap;

            SpawnLaserLine(line1Pos, soloTelegraphTime, soloLaserDamage, soloLaserDuration);
            SpawnLaserLine(line2Pos, soloTelegraphTime, soloLaserDamage, soloLaserDuration);

            float totalWait = soloTelegraphTime + soloLaserDuration + 2f;
            float t = 0f;
            while (t < totalWait)
            {
                rb.velocity = Vector2.zero;
                t += Time.deltaTime;
                yield return null;
            }
        }

        isPatternPlaying = false;
    }

    // 보스를 플레이어 기준 좌우로 offsetX만큼 이동시키는 함수
    private IEnumerator MoveToPlayerSide(bool isRightSide, float offsetX = 10f, float moveSpeed = 6f)
    {
        if (player == null) yield break;

        float targetX = player.position.x + (isRightSide ? offsetX : -offsetX);
        Vector3 target = new Vector3(targetX, transform.position.y, transform.position.z);

        while (Vector2.Distance(transform.position, target) > 0.1f)
        {
            Vector3 dir = (target - transform.position).normalized;
            rb.velocity = new Vector2(dir.x * moveSpeed, rb.velocity.y);

            FR = dir.x > 0;

            yield return null;
        }

        rb.velocity = Vector2.zero; // 멈춤
    }


    // ===================== 패턴4: 돌진 (붙으려 이동) =====================

    private IEnumerator Pattern4_ChargeToPlayer()
    {
        isPatternPlaying = true;
        rb.velocity = Vector2.zero;

        bool twinPhase = (!InSoloPhase && partnerHealth != null && !partnerDead);

        yield return new WaitForSeconds(0.3f);

        if (twinPhase)
        {
            Debug.Log($"[{name}] 패턴4: Twin 돌진 시작");

            isChargeAttackActive = true;
            currentChargeDashIndex = 0;

            float elapsed = 0f;
            while (elapsed < chargeDuration)
            {
                if (player != null)
                {
                    Vector2 dir = (player.position - transform.position).normalized;
                    rb.velocity = new Vector2(dir.x * chargeSpeed, rb.velocity.y);
                    FR = dir.x > 0;
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            rb.velocity = Vector2.zero;
            isChargeAttackActive = false;

            // 부딪힌 후 양쪽으로 느리게 이동
            if (player != null)
            {
                float outwardDirX = Mathf.Sign(transform.position.x - player.position.x); // 바깥 방향
                float t = 0f;
                while (t < outwardSlowDuration)
                {
                    rb.velocity = new Vector2(outwardSlowSpeed * outwardDirX, rb.velocity.y);
                    FR = outwardDirX > 0;
                    t += Time.deltaTime;
                    yield return null;
                }
            }

            rb.velocity = new Vector2(0f, rb.velocity.y);
        }
        else
        {
            Debug.Log($"[{name}] 패턴4: Solo 돌진 시작");

            int maxDash = 2;
            chargeHitThisDash = false;

            for (int dashIndex = 1; dashIndex <= maxDash; dashIndex++)
            {
                currentChargeDashIndex = dashIndex;
                isChargeAttackActive = true;
                chargeHitThisDash = false;

                float elapsed = 0f;
                while (elapsed < chargeDuration)
                {
                    if (player != null)
                    {
                        Vector2 dir = (player.position - transform.position).normalized;
                        rb.velocity = new Vector2(dir.x * chargeSpeed, rb.velocity.y);
                        FR = dir.x > 0;
                    }

                    elapsed += Time.deltaTime;
                    yield return null;
                }

                rb.velocity = Vector2.zero;
                isChargeAttackActive = false;

                if (dashIndex == 1)
                {
                    if (chargeHitThisDash)
                    {
                        Debug.Log($"[{name}] Solo 첫 돌진 히트 → 두 번째 돌진 준비");
                        yield return new WaitForSeconds(0.2f);
                        continue; // 두 번째 돌진
                    }
                    else
                    {
                        Debug.Log($"[{name}] Solo 첫 돌진 실패 → 패턴 종료");
                        break;
                    }
                }
                else
                {
                    Debug.Log($"[{name}] Solo 두 번째 돌진 종료");
                    break;
                }
            }

            // 마지막 위치에서 2초간 서 있기
            float wait = 0f;
            while (wait < 2f)
            {
                rb.velocity = Vector2.zero;
                wait += Time.deltaTime;
                yield return null;
            }
        }

        isPatternPlaying = false;
    }

    // ===================== Solo 회복 패턴 (파트너 죽자마자 자동 발동) =====================

    private IEnumerator CoSoloHealPhaseImmediate()
    {
        if (myHealth == null) yield break;

        isPatternPlaying = true;
        rb.velocity = Vector2.zero;

        Debug.Log($"[{name}] Solo Heal Phase 시작");

        // 원 이펙트 생성
        if (healCirclePrefab != null && healCircleInstance == null)
        {
            healCircleInstance = Instantiate(healCirclePrefab, transform.position, Quaternion.identity, transform);
        }

        // 힐 모드 ON
        myHealth.isHealMode = true;
        myHealth.healOnHitAmount = 5f; // 한 번 맞을 때 +5 (BossTwinHp 쪽에 필드 있어야 함)

        float elapsed = 0f;
        float interval = soloHealInterval;

        while (elapsed < soloHealDuration)
        {
            myHealth.currentHealth += soloHealAmountPerTick;
            if (myHealth.currentHealth > myHealth.maxHealth)
                myHealth.currentHealth = myHealth.maxHealth;

            myHealth.onHealthChanged?.Invoke(myHealth.currentHealth, myHealth.maxHealth);

            elapsed += interval;
            yield return new WaitForSeconds(interval);
        }

        // 힐 모드 OFF
        myHealth.isHealMode = false;

        if (healCircleInstance != null)
        {
            Destroy(healCircleInstance);
            healCircleInstance = null;
        }

        Debug.Log($"[{name}] Solo Heal Phase 종료 → 1초 정지");

        float waitStop = 0f;
        while (waitStop < 1f)
        {
            rb.velocity = Vector2.zero;
            waitStop += Time.deltaTime;
            yield return null;
        }

        isPatternPlaying = false;
    }

    // ===================== Twin / 파트너 이벤트 =====================

    private void OnPartnerDeath()
    {
        Debug.Log($"[{name}] 파트너 사망 → Solo Phase + 즉시 회복 패턴");

        partnerDead = true;

        if (!soloHealTriggered && myHealth != null && myHealth.currentHealth > 0f)
        {
            soloHealTriggered = true;
            StartCoroutine(CoSoloHealPhaseImmediate());
        }
    }

    // ===================== 충돌 처리 (점프 / 돌진) =====================

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag("Player")) return;

        var playerHP = collision.collider.GetComponent<PlayerAttack>();      // 체력 스크립트
        var playerMove = collision.collider.GetComponent<PlayerMovement>();  // 이동/슬로우/스턴 스크립트

        // --- 점프 패턴 ---
        if (isJumpAttackActive)
        {
            // 점프 중 스침
            if (!landingHitWindow)
            {
                if (playerHP != null)
                {
                    playerHP.TakeDamage(jumpTouchDamage);
                    Debug.Log("[JumpAttack] 점프 중 스침 → " + jumpTouchDamage + " 데미지");
                }
            }

            // 착지 직후
            if (landingHitWindow)
            {
                if (playerHP != null)
                {
                    playerHP.TakeDamage(landingHitDamage);
                    Debug.Log("[JumpAttack] 착지 타이밍 히트 → " + landingHitDamage + " 데미지 + 슬로우");
                }

                if (playerMove != null)
                {
                    playerMove.ApplySpeedDebuff(landingSlowAmount, 2f); // 2초 동안 -2
                }
            }

            return;
        }

        // --- 돌진 패턴 ---
        if (isChargeAttackActive)
        {
            bool twinPhase = (!InSoloPhase && partnerHealth != null && !partnerDead);

            if (twinPhase)
            {
                // Twin: 돌진 중 부딪히면 10 + 20, 스턴
                if (playerHP != null)
                {
                    playerHP.TakeDamage(twinChargeTouchDamageFirst);
                    playerHP.TakeDamage(twinChargeExtraDamageOnStun);
                    Debug.Log("[Charge-Twin] 돌진 히트 → 10 + 20 데미지");
                }

                if (playerMove != null)
                {
                    playerMove.Stun(twinChargeStunDuration);
                }
            }
            else
            {
                // Solo: 1번째 / 2번째 돌진에 따라 데미지 다르게
                if (playerHP != null)
                {
                    if (currentChargeDashIndex == 1)
                    {
                        playerHP.TakeDamage(soloChargeFirstHitDamage);
                        Debug.Log("[Charge-Solo] 첫 돌진 히트 → " + soloChargeFirstHitDamage + " 데미지");
                    }
                    else if (currentChargeDashIndex == 2)
                    {
                        playerHP.TakeDamage(soloChargeSecondHitDamage);
                        Debug.Log("[Charge-Solo] 두 번째 돌진 히트 → " + soloChargeSecondHitDamage + " 데미지");
                    }
                }

                chargeHitThisDash = true;
            }
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
