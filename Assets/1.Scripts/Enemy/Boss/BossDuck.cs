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

    [Header("Gravity & Ground")]
    [SerializeField] private float gravityScale = 4f;        // �� �߷� ũ��
    [SerializeField] private Transform groundCheckPoint;     // �߹� ����Ʈ(����). ������ �ڵ� ���
    [SerializeField] private float groundCheckYOffset = 0.05f; // �ڵ� ���� �߹����� ���� ���� ������
    [SerializeField] private float groundCheckRadius = 0.16f;  // �߹� �� �ݰ�
    [SerializeField] private LayerMask groundMask;           // Ground ���̾�
    private bool grounded;
    private float defaultGravity;

    [Header("Slam (�������: ������ ������ �ϰ�)")]
    [SerializeField] private float slamRiseHeight = 6f;
    [SerializeField] private float slamRiseTime = 0.25f;
    [SerializeField] private float hangTime = 0.15f;
    [SerializeField] private float stunAfterSlam = 2.5f;
    [SerializeField] private GameObject slamDecalPrefab;

    [Header("Shockwave Fire Points")]
    [SerializeField] private Transform firePointLeft;
    [SerializeField] private Transform firePointRight;
    [SerializeField] private GameObject shockwavePrefab;
    [SerializeField] private bool spawnShockwaveOnSlam = true;
    [SerializeField] private float shockwaveSideDelay = 0f;

    public enum TwoDAxis { Right, Up }
    [SerializeField] private TwoDAxis shockwaveForwardAxis = TwoDAxis.Right;

    [Header("Charge (����)")]
    [SerializeField] private float chargeWindup = 1f;
    [SerializeField] private float chargeSpeed = 10f;
    [SerializeField] private float chargeDuration = 0.8f;
    [SerializeField] private LayerMask wallMask;
    [SerializeField] private float chargeCollisionRadius = 0.4f;
    [SerializeField] private float chargeSkin = 0.2f;

    [Header("Summon (���� ��ȯ: A/B)")]
    [SerializeField] private GameObject minionPrefabA;
    [SerializeField] private GameObject minionPrefabB;
    [SerializeField, Range(0f, 1f)] private float minionAChance = 0.5f;
    [SerializeField] private int minionMin = 7;
    [SerializeField] private int minionMax = 12;
    [SerializeField] private float minionSpawnRadius = 2.5f;
    [SerializeField] private LayerMask spawnBlockMask;

    [Header("Pattern Cooldowns")]
    [SerializeField] private float slamCooldown = 10f;
    [SerializeField] private float chargeCooldown = 6f;
    [SerializeField] private float summonCooldown = 12f;

    [Header("Facing")]
    [SerializeField] private float facingFlipThreshold = 0.05f;
    [SerializeField] private bool artworkFacesRight = false;
    [SerializeField] private bool useScaleFlip = false;
    [SerializeField] private Transform gfxRoot;

    private BossState state = BossState.Idle;
    private Vector2 chargeDir;
    private float lastSlamTime = -999f;
    private float lastChargeTime = -999f;
    private float lastSummonTime = -999f;
    private bool _hitWallDuringCharge = false;

    // Animator hash
    static readonly int hMoveSpeed = Animator.StringToHash("moveSpeed");
    static readonly int hIsFlying = Animator.StringToHash("isFlying");
    static readonly int hQuak = Animator.StringToHash("trgQuak");
    static readonly int hLaugh = Animator.StringToHash("trgLaugh");

    private bool _flying;
    private SpriteRenderer[] _allSRs;
    private bool _facingRight = true;

    private void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!sr) sr = GetComponentInChildren<SpriteRenderer>();
        if (!anim) anim = GetComponentInChildren<Animator>();
        _allSRs = GetComponentsInChildren<SpriteRenderer>(true);
        if (!gfxRoot && sr) gfxRoot = sr.transform;
    }

    private void Start()
    {
        if (!player)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
            else StartCoroutine(FindPlayerLoop());
        }

        // �߷� ����ϵ��� ����
        defaultGravity = gravityScale <= 0f ? 1f : gravityScale;
        rb.gravityScale = defaultGravity;
        rb.freezeRotation = true;

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
        // �׻� ���� Ground üũ
        grounded = CheckGrounded();

        if (!player)
        {
            // �߷��� �ڿ������� �۵�, ������ 0
            rb.velocity = new Vector2(0f, rb.velocity.y);
            UpdateLocomotionAnim(Mathf.Abs(rb.velocity.x));
            return;
        }

        switch (state)
        {
            case BossState.Chase:
                {
                    // �߷��� ����, X�ุ ����
                    float dx = player.position.x - transform.position.x;
                    float desiredX = Mathf.Sign(dx) * moveSpeed;
                    rb.velocity = new Vector2(desiredX, rb.velocity.y);

                    UpdateLocomotionAnim(Mathf.Abs(desiredX));
                    break;
                }

            case BossState.Jumping:
            case BossState.Charging:
            case BossState.Summoning:
                // ���� �߿��� �ڷ�ƾ�� �ӵ�/�߷� ����(�ʿ� �� isKinematic)
                UpdateLocomotionAnim(0f);
                break;

            case BossState.Stunned:
            case BossState.Idle:
                rb.velocity = new Vector2(0f, rb.velocity.y);
                UpdateLocomotionAnim(0f);
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

        // �ٶ󺸱�(�ø�)
        float dx2 = player.position.x - transform.position.x;
        if (dx2 > facingFlipThreshold) SetFacing(true);
        else if (dx2 < -facingFlipThreshold) SetFacing(false);
    }

    // �������������������������������� Ground üũ ��������������������������������
    private bool CheckGrounded()
    {
        Vector2 p;
        if (groundCheckPoint)
            p = groundCheckPoint.position;
        else
        {
            // �ݶ��̴��� ������ �ٴ��� ���, ������ Ʈ������ �Ʒ��� ������
            var col = GetComponent<Collider2D>();
            if (col)
                p = new Vector2(col.bounds.center.x, col.bounds.min.y - 0.01f);
            else
                p = (Vector2)transform.position + Vector2.down * groundCheckYOffset;
        }

        bool hit = Physics2D.OverlapCircle(p, groundCheckRadius, groundMask);
        return hit;
    }

    // �������������������������������� Slam ��������������������������������
    private IEnumerator CoSlam()
    {
        Debug.Log("[BossDuck] ���� ����: �������");
        lastSlamTime = Time.time;

        Vector2 targetPos = player ? (Vector2)player.position : (Vector2)transform.position;

        GameObject decal = null;
        if (slamDecalPrefab) decal = Instantiate(slamDecalPrefab, targetPos, Quaternion.identity);

        // ���� ���� ���� �̵�
        bool prevKinematic = rb.isKinematic;
        float prevGravity = rb.gravityScale;
        rb.isKinematic = true;
        rb.gravityScale = 0f;
        rb.velocity = Vector2.zero;

        // ��� ���� ����
        if (anim) anim.SetTrigger("slam_up");
        SetFlyAnim(true);

        // ���
        Vector2 startPos = rb.position;
        Vector2 apexPos = new Vector2(targetPos.x, targetPos.y + slamRiseHeight);
        yield return StartCoroutine(InterpPosition(startPos, apexPos, slamRiseTime));

        // ��� ���
        if (hangTime > 0f) yield return new WaitForSeconds(hangTime);

        // ���� ����(�׶��� ������ ����)
        yield return StartCoroutine(DropUntilGround(apexPos, 30f));

        // �����
        if (spawnShockwaveOnSlam && shockwavePrefab)
            yield return StartCoroutine(SpawnShockwavesFromFirePoints());

        // ���� ����
        if (anim) anim.SetTrigger("slam_land");
        SetFlyAnim(false);

        if (decal) Destroy(decal);

        state = BossState.Stunned;
        Debug.Log("[BossDuck] ���� ���� & ����");
        yield return new WaitForSeconds(stunAfterSlam);

        // ���� ����(�߷� ����)
        rb.isKinematic = prevKinematic;
        rb.gravityScale = prevGravity <= 0f ? defaultGravity : prevGravity;

        Debug.Log("[BossDuck] ���� ����: �������");
        state = BossState.Chase;
    }

    // ���/���� ����
    private IEnumerator InterpPosition(Vector2 from, Vector2 to, float duration, bool fastCurve = false)
    {
        float t = 0f;
        while (t < duration)
        {
            float a = t / duration;
            if (fastCurve) a = a * a;
            rb.position = Vector2.LerpUnclamped(from, to, a);
            t += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        rb.position = to;
    }

    // ���� ���� ������ ���� ����(���� ����)
    private IEnumerator DropUntilGround(Vector2 start, float fallSpeed)
    {
        float elapsed = 0f;
        Vector2 pos = start;
        rb.position = pos;

        float halfHeight = groundCheckRadius;
        var col2d = GetComponent<Collider2D>();
        if (col2d) halfHeight = col2d.bounds.extents.y;

        while (elapsed < 2f)
        {
            float step = fallSpeed * Time.fixedDeltaTime;
            Vector2 next = pos + Vector2.down * step;

            RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.down, step + groundCheckRadius, groundMask);
            if (hit.collider != null)
            {
                float snapY = hit.point.y + halfHeight * 0.98f;
                rb.position = new Vector2(pos.x, snapY);
                Debug.Log("[BossDuck] Ground ���� �� ���� ����");
                yield break;
            }

            rb.position = next;
            pos = next;

            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        // ���� �� ����
        RaycastHit2D fix = Physics2D.Raycast(rb.position, Vector2.down, 100f, groundMask);
        if (fix.collider != null)
        {
            float snapY = fix.point.y + halfHeight * 0.98f;
            rb.position = new Vector2(rb.position.x, snapY);
        }
    }

    // Shockwave
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

        if (!leftOk && !rightOk) { Debug.LogWarning("[BossDuck] Fire ����Ʈ ����"); yield break; }

        if (leftOk && shockwavePrefab)
            Instantiate(shockwavePrefab, firePointLeft.position, DirToRotation(Vector2.left));

        if (shockwaveSideDelay > 0f) yield return new WaitForSeconds(shockwaveSideDelay);

        if (rightOk && shockwavePrefab)
            Instantiate(shockwavePrefab, firePointRight.position, DirToRotation(Vector2.right));
    }

    // Charge
    private IEnumerator CoChargeSequence()
    {
        Debug.Log("[BossDuck] ���� ����: ����");
        lastChargeTime = Time.time;

        // X ���߰� Y�� �߷� ����
        rb.velocity = new Vector2(0f, rb.velocity.y);
        if (anim) anim.SetTrigger("charge_windup");
        yield return new WaitForSeconds(chargeWindup);

        yield return StartCoroutine(CoChargeOnce());

        if (_hitWallDuringCharge)
        {
            _hitWallDuringCharge = false;
            Debug.Log("[BossDuck] �� �浹 �� 2�� ����");
            yield return StartCoroutine(CoChargeOnce());
        }

        Debug.Log("[BossDuck] ���� ����: ����");
        state = BossState.Chase;
    }

    private IEnumerator CoChargeOnce()
    {
        chargeDir = player ? ((Vector2)(player.position - transform.position)).normalized
                           : ((sr && sr.flipX) ? Vector2.left : Vector2.right);

        if (anim) anim.SetTrigger("charge");

        float t = 0f;
        while (t < chargeDuration)
        {
            // ������ ���� ���ַ�, ������ �߷� ����
            float x = chargeDir.x * chargeSpeed;
            rb.velocity = new Vector2(x, rb.velocity.y);

            float dist = Mathf.Abs(x) * Time.fixedDeltaTime + chargeSkin;
            RaycastHit2D hit = Physics2D.CircleCast(rb.position, chargeCollisionRadius, Mathf.Sign(x) * Vector2.right, dist, wallMask);
            if (hit.collider != null)
            {
                Vector2 snapPos = hit.point - Mathf.Sign(x) * Vector2.right * (chargeCollisionRadius + 0.02f);
                rb.position = snapPos;
                _hitWallDuringCharge = true;
                break;
            }

            t += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        // X ����, Y�� �״��
        rb.velocity = new Vector2(0f, rb.velocity.y);
        yield return null;
    }

    // Summon
    private IEnumerator CoSummon()
    {
        Debug.Log("[BossDuck] ���� ����: ��ȯ");
        lastSummonTime = Time.time;

        if (anim) anim.SetTrigger("summon");

        rb.velocity = new Vector2(0f, rb.velocity.y); //��ȯ ���� ���� ���� �̵� ����

        yield return new WaitForSeconds(0.4f);
        if (!minionPrefabA && !minionPrefabB)
        { Debug.LogWarning("[BossDuck] ��ȯ ����"); state = BossState.Chase; yield break; }
        if (!minionPrefabA) { minionAChance = 0f; }
        if (!minionPrefabB) { minionAChance = 1f; }

        int count = Random.Range(minionMin, minionMax + 1);
        int tries = 0, spawned = 0, aSpawned = 0, bSpawned = 0;
        float bossY = transform.position.y;

        while (spawned < count && tries < count * 10)
        {
            tries++;
            float xOffset = Random.Range(-minionSpawnRadius, minionSpawnRadius);
            if (Mathf.Abs(xOffset) < 0.5f)
                xOffset = 0.5f * Mathf.Sign(xOffset == 0f ? (Random.value - 0.5f) : xOffset);

            Vector2 spawnPos = new Vector2(transform.position.x + xOffset, bossY);
            if (Physics2D.OverlapCircle(spawnPos, 0.3f, spawnBlockMask)) continue;

            bool pickA = Random.value < minionAChance;
            GameObject prefab = pickA ? minionPrefabA : minionPrefabB;
            if (!prefab) prefab = pickA ? minionPrefabB : minionPrefabA;
            if (!prefab) continue;

            var m = Instantiate(prefab, spawnPos, Quaternion.identity);
            Destroy(m, 5f);
            spawned++; if (prefab == minionPrefabA) aSpawned++; else bSpawned++;
        }

        Debug.Log($"[BossDuck] ��ȯ �Ϸ�: {spawned} (A:{aSpawned}, B:{bSpawned})");
        yield return new WaitForSeconds(0.4f);
        state = BossState.Chase;
    }

    // Facing
    private void SetFacing(bool faceRight)
    {
        if (_facingRight == faceRight) return;
        _facingRight = faceRight;

        bool wantFlipX = artworkFacesRight ? !faceRight : faceRight;

        if (!useScaleFlip)
        {
            if (_allSRs != null)
                for (int i = 0; i < _allSRs.Length; i++) _allSRs[i].flipX = wantFlipX;
        }
        else if (gfxRoot)
        {
            Vector3 s = gfxRoot.localScale;
            s.x = Mathf.Abs(s.x) * (wantFlipX ? -1f : 1f);
            gfxRoot.localScale = s;
        }
    }

    private void UpdateLocomotionAnim(float speed)
    {
        if (!anim) return;
        anim.SetFloat(hMoveSpeed, speed);
    }

    private void SetFlyAnim(bool on)
    {
        if (!anim) return;
        if (_flying == on) return;
        _flying = on;
        anim.SetBool(hIsFlying, on);
    }

    public void PlayQuak() { if (anim) anim.SetTrigger(hQuak); }
    public void PlayLaugh() { if (anim) anim.SetTrigger(hLaugh); }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Ground gizmo
        Gizmos.color = Color.yellow;
        Vector2 p;
        if (groundCheckPoint) p = groundCheckPoint.position;
        else
        {
            var col = GetComponent<Collider2D>();
            p = col ? new Vector2(col.bounds.center.x, col.bounds.min.y - 0.01f)
                    : (Vector2)transform.position + Vector2.down * groundCheckYOffset;
        }
        Gizmos.DrawWireSphere(p, groundCheckRadius);

        // Fire points
        Gizmos.color = Color.cyan;
        if (firePointLeft) Gizmos.DrawSphere(firePointLeft.position, 0.08f);
        if (firePointRight) Gizmos.DrawSphere(firePointRight.position, 0.08f);
    }
#endif
}
