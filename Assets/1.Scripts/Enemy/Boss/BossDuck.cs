using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BossDuck : MonoBehaviour
{
    public enum BossState { Idle, Chase, Jumping, Charging, Summoning, Stunned }

    [Header("Refs")]
    [SerializeField] private Transform player;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Animator anim;

    [Header("Move")]
    [SerializeField] private float moveSpeed = 4f;

    [Header("Slam (내려찍기: 위에서 빠르게 하강)")]
    [Tooltip("정조준한 타겟 지점 위로 얼마나 올라갈지(수직 높이)")]
    [SerializeField] private float slamRiseHeight = 6f;
    [Tooltip("타겟 상공으로 상승하는 데 걸리는 시간")]
    [SerializeField] private float slamRiseTime = 0.25f;
    [Tooltip("상공에서 잠깐 멈추는 시간(연출용)")]
    [SerializeField] private float hangTime = 0.15f;
    [Tooltip("착지 후 멍해 있는 시간(이 동안 플레이어에게 기회 제공)")]
    [SerializeField] private float stunAfterSlam = 2.5f;
    [Tooltip("착지 지점 표시용 데칼(선택)")]
    [SerializeField] private GameObject slamDecalPrefab;

    [Header("Shockwave Fire Points")]
    [SerializeField] private Transform firePointLeft;
    [SerializeField] private Transform firePointRight;
    [SerializeField] private GameObject shockwavePrefab;
    [SerializeField] private bool spawnShockwaveOnSlam = true;
    [SerializeField] private float shockwaveSideDelay = 0f;

    public enum TwoDAxis { Right, Up } // 충격파 전진 방향 
    [Tooltip("충격파 프리팹이 전진할 때 사용하는 로컬 축(+X면 Right, +Y면 Up)")]
    [SerializeField] private TwoDAxis shockwaveForwardAxis = TwoDAxis.Right;

    [Header("Charge (돌진)")]
    [SerializeField] private float chargeWindup = 1f;
    [SerializeField] private float chargeSpeed = 10f;
    [SerializeField] private float chargeDuration = 0.8f;
    [SerializeField] private LayerMask wallMask;
    [SerializeField] private float chargeCollisionRadius = 0.4f;
    [SerializeField] private float chargeSkin = 0.2f;

    [Header("Summon (신하 소환: A/B)")]
    [SerializeField] private GameObject minionPrefabA;
    [SerializeField] private GameObject minionPrefabB;
    [SerializeField, Range(0f, 1f)] private float minionAChance = 0.5f;
    [SerializeField] private int minionMin = 7;
    [SerializeField] private int minionMax = 12;
    [SerializeField] private float minionSpawnRadius = 2.5f;
    [SerializeField] private LayerMask spawnBlockMask; //스폰 불가 레이어(벽/기둥/장식/금지영역 등)

    [Header("Pattern Cooldowns")]
    [SerializeField] private float slamCooldown = 10f;
    [SerializeField] private float chargeCooldown = 6f;
    [SerializeField] private float summonCooldown = 12f;

    [Header("FX/Etc")]
    [SerializeField] private float facingFlipThreshold = 0.05f;

    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundMask;           // Ground Layer 지정
    [SerializeField] private float groundCheckRadius = 0.4f; // 보스 발 밑 원반 체크 반경

    [Header("Slam Fall")]
    [SerializeField] private float slamFallSpeed = 30f;      // 낙하 속도
    [SerializeField] private float slamMaxFallSeconds = 2f;  // 안전 타임아웃(지면 못 찾을 때)

    private BossState state = BossState.Idle;
    private Vector2 chargeDir;
    private float lastSlamTime = -999f;
    private float lastChargeTime = -999f;
    private float lastSummonTime = -999f;
    private bool _hitWallDuringCharge = false;

    private void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!sr) sr = GetComponentInChildren<SpriteRenderer>();
        if (!anim) anim = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        if (!player)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
            else StartCoroutine(FindPlayerLoop());
        }
        //물리 세팅
        rb.gravityScale = 0f;       
        rb.freezeRotation = true;   // 회전 고정
        state = BossState.Chase;
    }

    private IEnumerator FindPlayerLoop()
    {
        while (!player)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
            yield return null;
        }
    }

    private void FixedUpdate()
    {
        if (!player) { rb.velocity = Vector2.zero; return; }

        switch (state)
        {
            case BossState.Chase:
                Vector2 toPlayer = (player.position - transform.position);
                rb.velocity = toPlayer.normalized * moveSpeed;
                if (anim) anim.SetFloat("speed", rb.velocity.magnitude);
                break;

            
            case BossState.Jumping:
            case BossState.Charging:
            case BossState.Summoning:          
                break;

            case BossState.Stunned:
            case BossState.Idle:
                rb.velocity = Vector2.zero;
                break;
        }
    }

    private void Update()
    {
        if (!player) return;

        if (state == BossState.Chase)
        {
            bool canSlam = Time.time - lastSlamTime >= slamCooldown;
            bool canCharge = Time.time - lastChargeTime >= chargeCooldown;
            bool canSummon = Time.time - lastSummonTime >= summonCooldown;

            if (canSlam || canCharge || canSummon)
            {
                int pick = Random.Range(0, 3);
                if (pick == 0 && canSlam) { state = BossState.Jumping; StartCoroutine(CoSlam()); return; }
                if (pick == 1 && canCharge) { state = BossState.Charging; StartCoroutine(CoChargeSequence()); return; }
                if (pick == 2 && canSummon) { state = BossState.Summoning; StartCoroutine(CoSummon()); return; }

                if (canSlam) { state = BossState.Jumping; StartCoroutine(CoSlam()); return; }
                if (canCharge) { state = BossState.Charging; StartCoroutine(CoChargeSequence()); return; }
                if (canSummon) { state = BossState.Summoning; StartCoroutine(CoSummon()); return; }
            }
        }

        // 좌우 바라보기
        var dir = player.position - transform.position;
        if (dir.x > facingFlipThreshold) sr.flipX = false;
        else if (dir.x < -facingFlipThreshold) sr.flipX = true;
    }

    // ─────────────────────── Slam (위에서 빠르게 낙하: Ground에 닿을 때까지) ───────────────────────

    private IEnumerator CoSlam()
    {
        Debug.Log("[BossDuck] 패턴 시작: 내려찍기");
        lastSlamTime = Time.time;

        Vector2 targetPos = player ? (Vector2)player.position : (Vector2)transform.position;

        GameObject decal = null;
        if (slamDecalPrefab)
            decal = Instantiate(slamDecalPrefab, targetPos, Quaternion.identity);

        bool prevKinematic = rb.isKinematic;
        rb.isKinematic = true;
        rb.velocity = Vector2.zero;

        // 1) 상승: 현재 (타겟.x, 타겟.y + 높이)
        if (anim) anim.SetTrigger("slam_up");
        Vector2 startPos = rb.position;
        Vector2 apexPos = new Vector2(targetPos.x, targetPos.y + slamRiseHeight);
        yield return StartCoroutine(InterpPosition(startPos, apexPos, slamRiseTime));

        // 2) 상공 잠깐 정지(연출)
        if (hangTime > 0f) yield return new WaitForSeconds(hangTime);

        // 3) Ground를 만날 때까지 계속 낙하 (시간 제한: slamMaxFallSeconds)
        yield return StartCoroutine(DropUntilGround(apexPos, slamFallSpeed));

        // 착지 처리 (DropUntilGround에서 rb.position을 설정함)
        if (spawnShockwaveOnSlam && shockwavePrefab)
            yield return StartCoroutine(SpawnShockwavesFromFirePoints());

        if (decal) Destroy(decal);

        state = BossState.Stunned;
        if (anim) anim.SetTrigger("slam_land");
        Debug.Log("[BossDuck] 슬램 착지 & 스턴");
        yield return new WaitForSeconds(stunAfterSlam);

        rb.isKinematic = prevKinematic;
        Debug.Log("[BossDuck] 패턴 종료: 내려찍기");
        state = BossState.Chase;
    }

    // 상승/수평 이동 등 시간 보간 유틸
    private IEnumerator InterpPosition(Vector2 from, Vector2 to, float duration, bool fastCurve = false)
    {
        float t = 0f;
        while (t < duration)
        {
            float alpha = t / duration;
            if (fastCurve) alpha = alpha * alpha;

            Vector2 nextPos = Vector2.LerpUnclamped(from, to, alpha);
            rb.position = nextPos;

            t += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        rb.position = to;
    }

    // 지면 만날 때까지 지속 낙하 + 타임아웃 보정
    private IEnumerator DropUntilGround(Vector2 start, float fallSpeed)
    {
        float elapsed = 0f;
        Vector2 pos = start;
        rb.position = pos;

        // 현재 콜라이더의 절반 높이를 구해 스냅 높이로 사용
        float HalfHeight()
        {
            float hh = groundCheckRadius;
            var col2d = GetComponent<Collider2D>();
            if (col2d != null) hh = col2d.bounds.extents.y;
            return hh;
        }

        while (elapsed < slamMaxFallSeconds)
        {
            float step = fallSpeed * Time.fixedDeltaTime;
            Vector2 next = pos + Vector2.down * step;

            // 이동 경로만큼 레이캐스트해서 지면 히트 감지
            RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.down, step + groundCheckRadius, groundMask);
            if (hit.collider != null)
            {
                // 접점 바로 위로 스냅
                float snapY = hit.point.y + HalfHeight() * 0.98f;
                rb.position = new Vector2(pos.x, snapY);
                Debug.Log("[BossDuck] Ground Layer 감지 → 낙하 종료");
                yield break;
            }

            // 아직 지면 없음 계속 낙하
            rb.position = next;
            pos = next;

            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        // 타임아웃 보정
        Debug.LogWarning("[BossDuck] 낙하 타임아웃: 보정 레이 시도");
        RaycastHit2D fixHit = Physics2D.Raycast(rb.position, Vector2.down, 100f, groundMask);
        if (fixHit.collider != null)
        {
            float snapY = fixHit.point.y + (GetComponent<Collider2D>() ? GetComponent<Collider2D>().bounds.extents.y : groundCheckRadius) * 0.98f;
            rb.position = new Vector2(rb.position.x, snapY);
            Debug.Log("[BossDuck] 보정 레이 성공 → 강제 착지");
        }
        else
        {
            Debug.LogWarning("[BossDuck] 보정 실패: 지면을 찾지 못함(맵 레이어 설정 확인 필요)");
        }
    }

    // ─────────────────────── Shockwave (좌/우 자동 회전) ───────────────────────

    private Quaternion DirToRotation(Vector2 dir)
    {
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (shockwaveForwardAxis == TwoDAxis.Up) angle -= 90f;
        return Quaternion.Euler(0f, 0f, angle);
    }

    private IEnumerator SpawnShockwavesFromFirePoints()
    {
        bool leftOk = firePointLeft != null;
        bool rightOk = firePointRight != null;

        if (!leftOk && !rightOk)
        {
            Debug.LogWarning("[BossDuck] Fire 포인트가 비어 있습니다. 충격파 미생성.");
            yield break;
        }

        if (leftOk && shockwavePrefab)
        {
            Quaternion rotL = DirToRotation(Vector2.left);
            Instantiate(shockwavePrefab, firePointLeft.position, rotL);
            Debug.Log($"[BossDuck] Shockwave @Left (rotZ={rotL.eulerAngles.z:0.0}°)");
        }

        if (shockwaveSideDelay > 0f)
            yield return new WaitForSeconds(shockwaveSideDelay);

        if (rightOk && shockwavePrefab)
        {
            Quaternion rotR = DirToRotation(Vector2.right);
            Instantiate(shockwavePrefab, firePointRight.position, rotR);
            Debug.Log($"[BossDuck] Shockwave @Right (rotZ={rotR.eulerAngles.z:0.0}°)");
        }
    }

    // ─────────────────────── Charge (물리틱 동기화) ───────────────────────

    private IEnumerator CoChargeSequence()
    {
        Debug.Log("[BossDuck] 패턴 시작: 돌진");
        lastChargeTime = Time.time;

        rb.velocity = Vector2.zero;
        if (anim) anim.SetTrigger("charge_windup");
        yield return new WaitForSeconds(chargeWindup);

        yield return StartCoroutine(CoChargeOnce());

        if (_hitWallDuringCharge)
        {
            _hitWallDuringCharge = false;
            Debug.Log("[BossDuck] 벽 충돌 → 2차 돌진");
            yield return StartCoroutine(CoChargeOnce());
        }

        Debug.Log("[BossDuck] 패턴 종료: 돌진");
        state = BossState.Chase;
    }

    private IEnumerator CoChargeOnce()
    {
        if (player)
            chargeDir = ((Vector2)(player.position - transform.position)).normalized;
        else
            chargeDir = (sr && sr.flipX) ? Vector2.left : Vector2.right;

        if (anim) anim.SetTrigger("charge");

        float t = 0f;
        while (t < chargeDuration)
        {
            rb.velocity = chargeDir * chargeSpeed;

            float dist = chargeSpeed * Time.fixedDeltaTime + chargeSkin;
            RaycastHit2D hit = Physics2D.CircleCast(rb.position, chargeCollisionRadius, chargeDir, dist, wallMask);
            if (hit.collider != null)
            {
                // 벽 앞에 정확히 스냅(재충돌/박힘 방지)
                Vector2 snapPos = hit.point - chargeDir * (chargeCollisionRadius + 0.02f);
                rb.position = snapPos;
                _hitWallDuringCharge = true;
                Debug.Log("[BossDuck] 돌진 중 벽 감지 → 스냅 & 중단");
                break;
            }

            t += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        rb.velocity = Vector2.zero;
        yield return null;
    }

    // ─────────────────────── Summon (A/B 확률, 같은 높이, 5초 후 삭제) ───────────────────────

    private IEnumerator CoSummon()
    {
        Debug.Log("[BossDuck] 패턴 시작: 소환");
        lastSummonTime = Time.time;

        if (anim) anim.SetTrigger("summon");
        yield return new WaitForSeconds(0.4f);

        // 프리팹 가드
        if (!minionPrefabA && !minionPrefabB)
        {
            Debug.LogWarning("[BossDuck] 소환 실패, 미니언 프리팹 A와 B가 모두 비어 있음");
            state = BossState.Chase;
            yield break;
        }
        if (!minionPrefabA) { Debug.LogWarning("[BossDuck] A 프리팹 없음, B만 소환"); minionAChance = 0f; }
        if (!minionPrefabB) { Debug.LogWarning("[BossDuck] B 프리팹 없음, A만 소환"); minionAChance = 1f; }

        int count = Random.Range(minionMin, minionMax + 1);
        int tries = 0, spawned = 0;
        int aSpawned = 0, bSpawned = 0;

        float bossY = transform.position.y;

        while (spawned < count && tries < count * 10)
        {
            tries++;

            // 같은 높이에서만 스폰: 보스 y를 그대로 사용         
            float xOffset = Random.Range(-minionSpawnRadius, minionSpawnRadius);
            if (Mathf.Abs(xOffset) < 0.5f)
                xOffset = 0.5f * Mathf.Sign((xOffset == 0f) ? (Random.value - 0.5f) : xOffset);

            Vector2 spawnPos = new Vector2(transform.position.x + xOffset, bossY);

            // 스폰 금지 영역 체크(벽/기둥/장식/금지영역 등 통합 마스크)
            if (Physics2D.OverlapCircle(spawnPos, 0.3f, spawnBlockMask))
                continue;

            // 타입 선택
            bool pickA = Random.value < minionAChance;
            GameObject prefab = pickA ? minionPrefabA : minionPrefabB;
            if (!prefab) prefab = pickA ? minionPrefabB : minionPrefabA;
            if (!prefab) continue;

            GameObject newMinion = Instantiate(prefab, spawnPos, Quaternion.identity);
            Destroy(newMinion, 5f); // 5초 뒤 자동 삭제

            spawned++;
            if (prefab == minionPrefabA) aSpawned++; else bSpawned++;
        }

        Debug.Log($"[BossDuck] 소환 완료, 총 {spawned}마리 (A:{aSpawned}, B:{bSpawned}), 보스와 같은 높이에서 생성, 5초 후 삭제");
        yield return new WaitForSeconds(0.4f);
        state = BossState.Chase;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Ground 체크 반경 시각화
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, groundCheckRadius);

        // FirePoints 시각화
        if (firePointLeft) { Gizmos.color = Color.cyan; Gizmos.DrawSphere(firePointLeft.position, 0.08f); }
        if (firePointRight) { Gizmos.color = Color.cyan; Gizmos.DrawSphere(firePointRight.position, 0.08f); }
    }
#endif
}
