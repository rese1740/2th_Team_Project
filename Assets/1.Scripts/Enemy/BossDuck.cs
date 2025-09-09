using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BossDuck : MonoBehaviour
{
    public enum BossState { Idle, Chase, Attacking, Jumping, Charging, Summoning, Stunned }

    [Header("Refs")]
    [SerializeField] private Transform player;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Animator anim;

    [Header("Move")]
    [SerializeField] private float moveSpeed = 4f;

    [Header("Tile/Range")]
    [SerializeField] private float tileSize = 1f;
    [SerializeField] private float peckRangeTiles = 5f;
    private float PeckRangeWorld => peckRangeTiles * tileSize;

    [Header("Peck (쪼기)")]
    [SerializeField] private float peckDuration = 1.3f;
    [SerializeField] private float peckHitRadius = 1.0f;

    [Header("Slam (내려찍기)")]
    [SerializeField] private float hangTime = 3f;
    [SerializeField] private float stunAfterSlam = 2.5f;
    [SerializeField] private float slamKillRadius = 0.8f;
    [SerializeField] private GameObject slamDecalPrefab;
    [SerializeField] private GameObject shockwavePrefab;

    [Header("Charge (돌진)")]
    [SerializeField] private float chargeWindup = 1f;
    [SerializeField] private float chargeSpeed = 10f;
    [SerializeField] private float chargeDuration = 0.8f;
    [SerializeField] private LayerMask wallMask;

    [Header("Summon (신하 소환)")]
    [SerializeField] private GameObject minionPrefab;
    [SerializeField] private int minionMin = 7;
    [SerializeField] private int minionMax = 12;
    [SerializeField] private float minionSpawnRadius = 2.5f;

    [Header("Pattern Cooldowns")]
    [SerializeField] private float slamCooldown = 10f;
    [SerializeField] private float chargeCooldown = 6f;
    [SerializeField] private float summonCooldown = 12f;

    [Header("FX/Etc")]
    [SerializeField] private float facingFlipThreshold = 0.05f;

    private BossState state = BossState.Idle;
    private Vector2 chargeDir;
    private float lastSlamTime = -999f;
    private float lastChargeTime = -999f;
    private float lastSummonTime = -999f;
    private bool _hitWallDuringCharge = false;

    private void Reset()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
        anim = GetComponentInChildren<Animator>();
    }

    private void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!sr) sr = GetComponentInChildren<SpriteRenderer>();
        if (!anim) anim = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        if (!player) player = GameObject.FindGameObjectWithTag("Player")?.transform;
        state = BossState.Chase;
    }

    private void FixedUpdate()
    {
        if (!player) return;

        switch (state)
        {
            case BossState.Chase:
                ChaseTick();
                break;
            default:
                rb.velocity = Vector2.zero;
                break;
        }
    }

    private void Update()
    {
        if (!player) return;

        if (state == BossState.Chase)
        {
            if (Vector2.Distance(transform.position, player.position) <= PeckRangeWorld)
            {
                StartCoroutine(CoPeck());
                return;
            }

            bool canSlam = Time.time - lastSlamTime >= slamCooldown;
            bool canCharge = Time.time - lastChargeTime >= chargeCooldown;
            bool canSummon = Time.time - lastSummonTime >= summonCooldown;

            if (canSlam || canCharge || canSummon)
            {
                int pick = Random.Range(0, 3);
                if (pick == 0 && canSlam) { StartCoroutine(CoSlam()); return; }
                if (pick == 1 && canCharge) { StartCoroutine(CoChargeSequence()); return; }
                if (pick == 2 && canSummon) { StartCoroutine(CoSummon()); return; }

                if (canSlam) { StartCoroutine(CoSlam()); return; }
                if (canCharge) { StartCoroutine(CoChargeSequence()); return; }
                if (canSummon) { StartCoroutine(CoSummon()); return; }
            }
        }

        var dir = player.position - transform.position;
        if (dir.x > facingFlipThreshold) sr.flipX = false;
        else if (dir.x < -facingFlipThreshold) sr.flipX = true;
    }

    private void ChaseTick()
    {
        Vector2 toPlayer = (player.position - transform.position);
        rb.velocity = toPlayer.normalized * moveSpeed;
        if (anim) anim.SetFloat("speed", rb.velocity.magnitude);
    }

    // ─────────────────────── 공격들 ───────────────────────

    private IEnumerator CoPeck()
    {
        state = BossState.Attacking;
        rb.velocity = Vector2.zero;
        if (anim) anim.SetTrigger("peck");

        yield return new WaitForSeconds(peckDuration);

        state = BossState.Chase;
    }

    private IEnumerator CoSlam()
    {
        state = BossState.Jumping;
        lastSlamTime = Time.time;

        Vector2 targetPos = player.position;
        GameObject decal = null;
        if (slamDecalPrefab) decal = Instantiate(slamDecalPrefab, targetPos, Quaternion.identity);

        bool prevEnabled = sr.enabled;
        sr.enabled = false;
        rb.simulated = false;

        yield return new WaitForSeconds(hangTime);

        transform.position = targetPos;
        sr.enabled = prevEnabled;
        rb.simulated = true;

        if (shockwavePrefab) Instantiate(shockwavePrefab, targetPos, Quaternion.identity);
        if (decal) Destroy(decal);

        state = BossState.Stunned;
        if (anim) anim.SetTrigger("slam_land");
        yield return new WaitForSeconds(stunAfterSlam);

        state = BossState.Chase;
    }

    private IEnumerator CoChargeSequence()
    {
        state = BossState.Charging;
        lastChargeTime = Time.time;

        rb.velocity = Vector2.zero;
        if (anim) anim.SetTrigger("charge_windup");
        yield return new WaitForSeconds(chargeWindup);

        yield return StartCoroutine(CoChargeOnce());

        if (_hitWallDuringCharge)
        {
            _hitWallDuringCharge = false;
            yield return StartCoroutine(CoChargeOnce());
        }

        state = BossState.Chase;
    }

    private IEnumerator CoChargeOnce()
    {
        chargeDir = ((Vector2)(player.position - transform.position)).normalized;
        if (anim) anim.SetTrigger("charge");

        float t = 0f;
        while (t < chargeDuration)
        {
            rb.velocity = chargeDir * chargeSpeed;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, chargeDir, 0.6f, wallMask);
            if (hit.collider != null)
            {
                _hitWallDuringCharge = true;
                break;
            }
            t += Time.deltaTime;
            yield return null;
        }

        rb.velocity = Vector2.zero;
        yield return null;
    }

    private IEnumerator CoSummon()
    {
        state = BossState.Summoning;
        lastSummonTime = Time.time;

        if (anim) anim.SetTrigger("summon");
        yield return new WaitForSeconds(0.4f);

        int count = Random.Range(minionMin, minionMax + 1);
        for (int i = 0; i < count; i++)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * Random.Range(0.5f, minionSpawnRadius);
            Vector2 spawnPos = (Vector2)transform.position + offset;
            Instantiate(minionPrefab, spawnPos, Quaternion.identity);
        }

        yield return new WaitForSeconds(0.4f);
        state = BossState.Chase;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.25f);
        Gizmos.DrawWireSphere(transform.position, PeckRangeWorld);
    }
#endif
}
