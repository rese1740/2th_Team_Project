using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BoundaryEnemy : MonoBehaviour
{
    public enum State { Patrol, Alert, Chase, Attack }

    [Header("�ٿ���� �� So�ҷ�����")]
    public EnemySO config;

    [Header("�� ����")]
    public Transform firePoint;             // �߻� ����(�ѱ�)
    public SpriteRenderer spriteRenderer;   // �� ��������Ʈ ������
    public Transform gfxRoot;

    [Header("���� ����")]
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.2f; // �ٴ� ���� �Ÿ�
    public float airGravityScale = 3f;       // ������ �� �߷�
    public float groundGravityScale = 0f;    // Ground ���� �� �߷�

    private Rigidbody2D rb;                 // ���� �ٵ�
    private Transform player;               // �÷��̾� Transform

    private bool movingRight = true;        // ���� �ٶ󺸴� ����(true=����)
    private bool isAlerting = false;        // ��� ���� �� ����(�ߺ� ����)
    private bool hasAggro = false;          // �÷��̾�� ��׷ΰ� �پ�����

    private Coroutine attackRoutine;        // ���� ���� �ڷ�ƾ �ڵ�
    private float nextShotTime = 0f;        // ��ٿ� ����

    private State state = State.Patrol;     // ���� ����
    private bool isOnGround = false;        // Ground ���� ���

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // ���� �ڵ� ����
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (gfxRoot == null)
            gfxRoot = spriteRenderer != null ? spriteRenderer.transform : transform;

        if (firePoint == null)
            firePoint = transform;
    }

    void Start()
    {
        // �÷��̾� ����
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void OnDisable()
    {
        // ��Ȱ��ȭ �� ���� ����
        StopAttackLoop();
        rb.velocity = Vector2.zero;
        hasAggro = false;
    }

    void FixedUpdate()
    {
        // �� ���� �����Ӹ��� ���� ���� + �̵� + ������ + ���� ����
        bool inRange = IsPlayerInRange();

        switch (state)
        {
            case State.Patrol:
                // ���� ��׷ΰ� ���� Ž�� ���� �ȿ� ������ ���� ����
                if (!hasAggro && inRange && !isAlerting)
                    StartCoroutine(EnterAlert());
                break;

            case State.Alert:
                // ���� �� �߰�
                break;
            case State.Chase:
                if (inRange)
                { 
                    // �����Ÿ� ������
                    state = State.Attack;
                }


                else if (!config.persistAggro)
                {
                    // ��׷� ���� �ɼ��� ���� ������ ����
                    state = State.Patrol;
                    hasAggro = false;
                }
                break;
            case State.Attack:
                if (!inRange)
                {
                    // �����Ÿ� ��Ż > �߰�
                    state = State.Chase;
                    StopAttackLoop();   // �߻� ��� ����
                }
                break;
        }

        MoveByState();      // ���¿� ���� �̵�/�ӵ�/���� ���� ó��
        ApplyFlip();        // �ٶ󺸴� ���⿡ �°� ��������Ʈ/���̾�����Ʈ ����
        CheckGround();     // Ground ����(���� ���� �߷� 0, �ƴϸ� �߷� ����) 
    }

    // �÷��̾ Ž�� ���� �ȿ� �ִ��� Ȯ��
    private bool IsPlayerInRange()
    {
        if (player == null || config == null) return false;
        float r = config.detectRangeTiles * config.tileSize;
        return Vector2.Distance(transform.position, player.position) <= r;
    }

    // ��� ���� ����(����ǥ ������ + ���� �ִϸ��̼� + ��� �� �߰����� ����)
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

        // ��� ����(����)
        yield return StartCoroutine(BounceAnimation());
        yield return new WaitForSeconds(config.alertDuration);


        // �߰� ����
        hasAggro = true;
        state = State.Chase;
        isAlerting = false;
    }

    // �ٿ �ִϸ��̼�
    private IEnumerator BounceAnimation()
    {
        if (config == null) yield break;

        float half = 0.5f / Mathf.Max(0.0001f, config.bounceSpeed);
        float elapsed = 0f;

        Vector3 start = gfxRoot.localPosition;
        Vector3 peak = start + Vector3.up * config.bounceHeight;

        // ����
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / half);
            gfxRoot.localPosition = Vector3.Lerp(start, peak, t);
            yield return null;
        }

        // �Ʒ���
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

    //���º� �̵�/�ӵ�/���� ���� ����
    private void MoveByState()
    {
        if (config == null) return;

        switch (state)
        {
            case State.Patrol:
                // �¿� ����
                rb.velocity = new Vector2((movingRight ? 1f : -1f) * config.patrolSpeed, rb.velocity.y);
                StopAttackLoop();
                break;

            case State.Alert:
                // ��� �߿��� ����
                rb.velocity = Vector2.zero;
                StopAttackLoop();
                break;

            case State.Chase:
                // �߰� ���¿����� ���� ���� �ߴ�, �ɼǿ� ���� �÷��̾� ������ x �̵�
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
                // ���� ����: �ɼǿ� ���� �̵��� �����ϸ� ����ϰų� ���ڸ� ���
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
                StartAttackLoop(); // ���� ���� ����
                break;
        }
    }

    // ���� ���� ����
    private void StartAttackLoop()
    {
        if (attackRoutine == null)
            attackRoutine = StartCoroutine(AttackLoop());
    }


    // ���� ���� ����
    private void StopAttackLoop()
    {
        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }
    }

    // ���� ����: ��ٿ��� ����Ͽ� �ݺ� �߻�
    private IEnumerator AttackLoop()
    {
        while (state == State.Attack)
        {
            if (IsPlayerInRange() && Time.time >= nextShotTime)
            {
                FireOnce();
                nextShotTime = Time.time + Mathf.Max(0f, config.attackInterval);
            }
            yield return null;  // �� ������ üũ
        }
        attackRoutine = null;
    }


    // 1ȸ �߻� ó��
    private void FireOnce()
    {
        if (config == null || config.projectilePrefab == null || player == null)
            return;

        Vector2 dir = (player.position - firePoint.position);

        // ���� ���� �߻� ��� 
        if (config.attackOnlyHorizontal)
        {
            float sx = Mathf.Sign(dir.x);
            if (sx == 0) sx = movingRight ? 1f : -1f;
            dir = new Vector2(sx, 0f);
        }

        dir = dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector2.right;
        movingRight = dir.x >= 0f;

        // ����ü ����
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

    // ��� Ʈ���ſ� ������ ���� ���� ����
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (state == State.Patrol && collision.CompareTag("Boundary"))
            movingRight = !movingRight;
    }

    // �ٶ󺸴� ���⿡ �°� ��������Ʈ �� �߻�����Ʈ ����
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

    // Ground ���� ����
    private void CheckGround()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);

        if (hit.collider != null)
        {
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

    private void OnDrawGizmosSelected()
    {
        // Ž�� ���� ǥ��
        if (config != null)
        {
            Gizmos.color = Color.yellow;
            float r = config.detectRangeTiles * config.tileSize;
            Gizmos.DrawWireSphere(transform.position, r);
        }

        // �߻� ���� ǥ��
        if (firePoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(firePoint.position, 0.05f);
        }

        // �ٴ� ���� ���� �ð�ȭ
        Gizmos.color = isOnGround ? Color.green : Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
    }
}
