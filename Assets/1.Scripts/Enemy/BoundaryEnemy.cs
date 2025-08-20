using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BoundaryEnemy : MonoBehaviour
{
    [Header("Config (ScriptableObject)")]
    public EnemySO config;

    [Header("Scene Refs")]
    public Transform firePoint;
    public SpriteRenderer spriteRenderer;
    public Transform gfxRoot;

    private Rigidbody2D rb;
    private bool movingRight = true;
    private Transform player;

    // ����
    private enum State { Patrol, Alert, Chase, Attack }
    private State state = State.Patrol;

    // �÷���/�ڷ�ƾ
    private bool isAlerting = false;
    private bool hasAggro = false;
    private Coroutine attackRoutine;
    private float nextShotTime = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (gfxRoot == null) gfxRoot = spriteRenderer != null ? spriteRenderer.transform : transform;
        if (firePoint == null) firePoint = transform; // ������ġ
    }

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void OnDisable()
    {
        StopAttackLoop();
        rb.velocity = Vector2.zero;
        // �ʿ� �� ���� �ʱ�ȭ:
        // state = State.Patrol; hasAggro = false;
    }

    void FixedUpdate()
    {
        bool inRange = IsPlayerInRange();

        switch (state)
        {
            case State.Patrol:
                if (!hasAggro && inRange && !isAlerting)
                    StartCoroutine(EnterAlert());
                break;

            case State.Alert:
                // ���̴� �ڷ�ƾ���� ó��
                break;

            case State.Chase:
                if (inRange)
                {
                    state = State.Attack;    // ��Ÿ� ������ �� ����
                }
                else
                {
                    if (!config.persistAggro)
                    {
                        state = State.Patrol; // ��׷� �������� ����
                        hasAggro = false;
                    }
                }
                break;

            case State.Attack:
                if (!inRange)
                {
                    state = State.Chase;     // ��Ÿ� ��Ż �� �߰ݸ�
                    StopAttackLoop();        // ��� �߻� �ߴ�
                }
                break;
        }

        MoveByState();
        ApplyFlip();
    }

    private bool IsPlayerInRange()
    {
        if (player == null || config == null) return false;
        float r = config.detectRangeTiles * config.tileSize;
        return Vector2.Distance(transform.position, player.position) <= r;
    }

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

        yield return StartCoroutine(BounceAnimation());
        yield return new WaitForSeconds(config.alertDuration);

        // �߰� �Ϸ� �� ��׷� ON �� Chase ����
        hasAggro = true;
        state = State.Chase;
        isAlerting = false;
    }

    private IEnumerator BounceAnimation()
    {
        if (config == null) yield break;

        float half = 0.5f / Mathf.Max(0.0001f, config.bounceSpeed);
        float elapsed = 0f;
        Vector3 start = gfxRoot.localPosition;
        Vector3 peak = start + Vector3.up * config.bounceHeight;

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
        if (config == null) return;

        switch (state)
        {
            case State.Patrol:
                rb.velocity = new Vector2((movingRight ? 1f : -1f) * config.patrolSpeed, rb.velocity.y);
                StopAttackLoop(); // Ȥ�� �� �ܿ� ���� ����
                break;

            case State.Alert:
                rb.velocity = Vector2.zero;
                StopAttackLoop();
                break;

            case State.Chase:
                StopAttackLoop(); // Chase������ ��� ����
                if (config.chaseWhileAggro && hasAggro && player != null)
                {
                    float dirX = Mathf.Sign(player.position.x - transform.position.x);
                    rb.velocity = new Vector2(dirX * config.chaseSpeed, rb.velocity.y);
                    movingRight = dirX >= 0f;
                }
                else
                {
                    rb.velocity = Vector2.zero;
                }
                break;

            case State.Attack:
                if (config.chaseWhileAggro && hasAggro && player != null)
                {
                    float dirX = Mathf.Sign(player.position.x - transform.position.x);
                    rb.velocity = new Vector2(dirX * config.chaseSpeed, rb.velocity.y); // ���� �� �߰� ����(�ɼ�)
                    movingRight = dirX >= 0f;
                }
                else
                {
                    rb.velocity = Vector2.zero; // ���ڸ� ���
                }

                StartAttackLoop(); // ���� ���� ����
                break;
        }
    }

    // ===== �߻� ���� ���� =====
    private void StartAttackLoop()
    {
        if (attackRoutine == null)
            attackRoutine = StartCoroutine(AttackLoop());
    }

    private void StopAttackLoop()
    {
        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }
    }

    private IEnumerator AttackLoop()
    {
        while (state == State.Attack)
        {
            // ��Ÿ� �ȿ����� ���
            if (IsPlayerInRange())
            {
                // ��ٿ� ����: ���� ȣ��/��� �����ӿ����� �ߺ� �߻� ����
                if (Time.time >= nextShotTime)
                {
                    FireOnce();
                    nextShotTime = Time.time + Mathf.Max(0f, config.attackInterval);
                }
            }
            yield return null; // �� ������ üũ
        }
        attackRoutine = null;
    }

    private void FireOnce()
    {
        if (config == null || config.projectilePrefab == null || player == null) return;

        Vector2 dir = (player.position - firePoint.position);
        if (config.attackOnlyHorizontal)
        {
            float sx = Mathf.Sign(dir.x);
            if (sx == 0) sx = movingRight ? 1f : -1f;
            dir = new Vector2(sx, 0f);
        }
        dir = dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector2.right;

        movingRight = dir.x >= 0f;

        GameObject proj = Instantiate(config.projectilePrefab, firePoint.position, Quaternion.identity);
        var bullet = proj.GetComponent<BELBullet>();
        if (bullet != null)
        {
            bullet.Init(dir, config.projectileSpeed);
        }
        else
        {
            var prb = proj.GetComponent<Rigidbody2D>();
            if (prb != null) prb.velocity = dir * config.projectileSpeed;
            Destroy(proj, 5f); // ���� ����
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

    private void OnDrawGizmosSelected()
    {
        if (config == null) return;

        Gizmos.color = Color.yellow;
        float r = config.detectRangeTiles * config.tileSize;
        Gizmos.DrawWireSphere(transform.position, r);

        if (firePoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(firePoint.position, 0.05f);
        }
    }
}