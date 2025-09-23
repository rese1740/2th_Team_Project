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

    [Header("Slam (�������: ������ ������ �ϰ�)")]
    [Tooltip("�������� Ÿ�� ���� ���� �󸶳� �ö���(���� ����)")]
    [SerializeField] private float slamRiseHeight = 6f;
    [Tooltip("Ÿ�� ������� ����ϴ� �� �ɸ��� �ð�")]
    [SerializeField] private float slamRiseTime = 0.25f;
    [Tooltip("������� ��� ���ߴ� �ð�(�����)")]
    [SerializeField] private float hangTime = 0.15f;
    [Tooltip("���� �� ���� �ִ� �ð�(�� ���� �÷��̾�� ��ȸ ����)")]
    [SerializeField] private float stunAfterSlam = 2.5f;
    [Tooltip("���� ���� ǥ�ÿ� ��Į(����)")]
    [SerializeField] private GameObject slamDecalPrefab;

    [Header("Shockwave Fire Points")]
    [SerializeField] private Transform firePointLeft;
    [SerializeField] private Transform firePointRight;
    [SerializeField] private GameObject shockwavePrefab;
    [SerializeField] private bool spawnShockwaveOnSlam = true;
    [SerializeField] private float shockwaveSideDelay = 0f;

    public enum TwoDAxis { Right, Up } // ����� ���� ���� 
    [Tooltip("����� �������� ������ �� ����ϴ� ���� ��(+X�� Right, +Y�� Up)")]
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
    [SerializeField] private LayerMask spawnBlockMask; //���� �Ұ� ���̾�(��/���/���/�������� ��)

    [Header("Pattern Cooldowns")]
    [SerializeField] private float slamCooldown = 10f;
    [SerializeField] private float chargeCooldown = 6f;
    [SerializeField] private float summonCooldown = 12f;

    [Header("FX/Etc")]
    [SerializeField] private float facingFlipThreshold = 0.05f;

    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundMask;           // Ground Layer ����
    [SerializeField] private float groundCheckRadius = 0.4f; // ���� �� �� ���� üũ �ݰ�

    [Header("Slam Fall")]
    [SerializeField] private float slamFallSpeed = 30f;      // ���� �ӵ�
    [SerializeField] private float slamMaxFallSeconds = 2f;  // ���� Ÿ�Ӿƿ�(���� �� ã�� ��)

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
        //���� ����
        rb.gravityScale = 0f;       
        rb.freezeRotation = true;   // ȸ�� ����
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

        // �¿� �ٶ󺸱�
        var dir = player.position - transform.position;
        if (dir.x > facingFlipThreshold) sr.flipX = false;
        else if (dir.x < -facingFlipThreshold) sr.flipX = true;
    }

    // ���������������������������������������������� Slam (������ ������ ����: Ground�� ���� ������) ����������������������������������������������

    private IEnumerator CoSlam()
    {
        Debug.Log("[BossDuck] ���� ����: �������");
        lastSlamTime = Time.time;

        Vector2 targetPos = player ? (Vector2)player.position : (Vector2)transform.position;

        GameObject decal = null;
        if (slamDecalPrefab)
            decal = Instantiate(slamDecalPrefab, targetPos, Quaternion.identity);

        bool prevKinematic = rb.isKinematic;
        rb.isKinematic = true;
        rb.velocity = Vector2.zero;

        // 1) ���: ���� (Ÿ��.x, Ÿ��.y + ����)
        if (anim) anim.SetTrigger("slam_up");
        Vector2 startPos = rb.position;
        Vector2 apexPos = new Vector2(targetPos.x, targetPos.y + slamRiseHeight);
        yield return StartCoroutine(InterpPosition(startPos, apexPos, slamRiseTime));

        // 2) ��� ��� ����(����)
        if (hangTime > 0f) yield return new WaitForSeconds(hangTime);

        // 3) Ground�� ���� ������ ��� ���� (�ð� ����: slamMaxFallSeconds)
        yield return StartCoroutine(DropUntilGround(apexPos, slamFallSpeed));

        // ���� ó�� (DropUntilGround���� rb.position�� ������)
        if (spawnShockwaveOnSlam && shockwavePrefab)
            yield return StartCoroutine(SpawnShockwavesFromFirePoints());

        if (decal) Destroy(decal);

        state = BossState.Stunned;
        if (anim) anim.SetTrigger("slam_land");
        Debug.Log("[BossDuck] ���� ���� & ����");
        yield return new WaitForSeconds(stunAfterSlam);

        rb.isKinematic = prevKinematic;
        Debug.Log("[BossDuck] ���� ����: �������");
        state = BossState.Chase;
    }

    // ���/���� �̵� �� �ð� ���� ��ƿ
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

    // ���� ���� ������ ���� ���� + Ÿ�Ӿƿ� ����
    private IEnumerator DropUntilGround(Vector2 start, float fallSpeed)
    {
        float elapsed = 0f;
        Vector2 pos = start;
        rb.position = pos;

        // ���� �ݶ��̴��� ���� ���̸� ���� ���� ���̷� ���
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

            // �̵� ��θ�ŭ ����ĳ��Ʈ�ؼ� ���� ��Ʈ ����
            RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.down, step + groundCheckRadius, groundMask);
            if (hit.collider != null)
            {
                // ���� �ٷ� ���� ����
                float snapY = hit.point.y + HalfHeight() * 0.98f;
                rb.position = new Vector2(pos.x, snapY);
                Debug.Log("[BossDuck] Ground Layer ���� �� ���� ����");
                yield break;
            }

            // ���� ���� ���� ��� ����
            rb.position = next;
            pos = next;

            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        // Ÿ�Ӿƿ� ����
        Debug.LogWarning("[BossDuck] ���� Ÿ�Ӿƿ�: ���� ���� �õ�");
        RaycastHit2D fixHit = Physics2D.Raycast(rb.position, Vector2.down, 100f, groundMask);
        if (fixHit.collider != null)
        {
            float snapY = fixHit.point.y + (GetComponent<Collider2D>() ? GetComponent<Collider2D>().bounds.extents.y : groundCheckRadius) * 0.98f;
            rb.position = new Vector2(rb.position.x, snapY);
            Debug.Log("[BossDuck] ���� ���� ���� �� ���� ����");
        }
        else
        {
            Debug.LogWarning("[BossDuck] ���� ����: ������ ã�� ����(�� ���̾� ���� Ȯ�� �ʿ�)");
        }
    }

    // ���������������������������������������������� Shockwave (��/�� �ڵ� ȸ��) ����������������������������������������������

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
            Debug.LogWarning("[BossDuck] Fire ����Ʈ�� ��� �ֽ��ϴ�. ����� �̻���.");
            yield break;
        }

        if (leftOk && shockwavePrefab)
        {
            Quaternion rotL = DirToRotation(Vector2.left);
            Instantiate(shockwavePrefab, firePointLeft.position, rotL);
            Debug.Log($"[BossDuck] Shockwave @Left (rotZ={rotL.eulerAngles.z:0.0}��)");
        }

        if (shockwaveSideDelay > 0f)
            yield return new WaitForSeconds(shockwaveSideDelay);

        if (rightOk && shockwavePrefab)
        {
            Quaternion rotR = DirToRotation(Vector2.right);
            Instantiate(shockwavePrefab, firePointRight.position, rotR);
            Debug.Log($"[BossDuck] Shockwave @Right (rotZ={rotR.eulerAngles.z:0.0}��)");
        }
    }

    // ���������������������������������������������� Charge (����ƽ ����ȭ) ����������������������������������������������

    private IEnumerator CoChargeSequence()
    {
        Debug.Log("[BossDuck] ���� ����: ����");
        lastChargeTime = Time.time;

        rb.velocity = Vector2.zero;
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
                // �� �տ� ��Ȯ�� ����(���浹/���� ����)
                Vector2 snapPos = hit.point - chargeDir * (chargeCollisionRadius + 0.02f);
                rb.position = snapPos;
                _hitWallDuringCharge = true;
                Debug.Log("[BossDuck] ���� �� �� ���� �� ���� & �ߴ�");
                break;
            }

            t += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        rb.velocity = Vector2.zero;
        yield return null;
    }

    // ���������������������������������������������� Summon (A/B Ȯ��, ���� ����, 5�� �� ����) ����������������������������������������������

    private IEnumerator CoSummon()
    {
        Debug.Log("[BossDuck] ���� ����: ��ȯ");
        lastSummonTime = Time.time;

        if (anim) anim.SetTrigger("summon");
        yield return new WaitForSeconds(0.4f);

        // ������ ����
        if (!minionPrefabA && !minionPrefabB)
        {
            Debug.LogWarning("[BossDuck] ��ȯ ����, �̴Ͼ� ������ A�� B�� ��� ��� ����");
            state = BossState.Chase;
            yield break;
        }
        if (!minionPrefabA) { Debug.LogWarning("[BossDuck] A ������ ����, B�� ��ȯ"); minionAChance = 0f; }
        if (!minionPrefabB) { Debug.LogWarning("[BossDuck] B ������ ����, A�� ��ȯ"); minionAChance = 1f; }

        int count = Random.Range(minionMin, minionMax + 1);
        int tries = 0, spawned = 0;
        int aSpawned = 0, bSpawned = 0;

        float bossY = transform.position.y;

        while (spawned < count && tries < count * 10)
        {
            tries++;

            // ���� ���̿����� ����: ���� y�� �״�� ���         
            float xOffset = Random.Range(-minionSpawnRadius, minionSpawnRadius);
            if (Mathf.Abs(xOffset) < 0.5f)
                xOffset = 0.5f * Mathf.Sign((xOffset == 0f) ? (Random.value - 0.5f) : xOffset);

            Vector2 spawnPos = new Vector2(transform.position.x + xOffset, bossY);

            // ���� ���� ���� üũ(��/���/���/�������� �� ���� ����ũ)
            if (Physics2D.OverlapCircle(spawnPos, 0.3f, spawnBlockMask))
                continue;

            // Ÿ�� ����
            bool pickA = Random.value < minionAChance;
            GameObject prefab = pickA ? minionPrefabA : minionPrefabB;
            if (!prefab) prefab = pickA ? minionPrefabB : minionPrefabA;
            if (!prefab) continue;

            GameObject newMinion = Instantiate(prefab, spawnPos, Quaternion.identity);
            Destroy(newMinion, 5f); // 5�� �� �ڵ� ����

            spawned++;
            if (prefab == minionPrefabA) aSpawned++; else bSpawned++;
        }

        Debug.Log($"[BossDuck] ��ȯ �Ϸ�, �� {spawned}���� (A:{aSpawned}, B:{bSpawned}), ������ ���� ���̿��� ����, 5�� �� ����");
        yield return new WaitForSeconds(0.4f);
        state = BossState.Chase;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Ground üũ �ݰ� �ð�ȭ
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, groundCheckRadius);

        // FirePoints �ð�ȭ
        if (firePointLeft) { Gizmos.color = Color.cyan; Gizmos.DrawSphere(firePointLeft.position, 0.08f); }
        if (firePointRight) { Gizmos.color = Color.cyan; Gizmos.DrawSphere(firePointRight.position, 0.08f); }
    }
#endif
}
