using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BoundaryEnemyS : MonoBehaviour
{
    [Header("Speed")]
    public float patrolSpeed = 3f;
    public float chaseSpeed = 4.5f;

    [Header("Detection")]
    public float detectRangeTiles = 5f;
    public float tileSize = 1f;

    [Header("Alert")]
    public GameObject alertIconPrefab;
    public Vector3 alertOffset = new Vector3(0, 1.5f, 0);
    public float alertDuration = 0.3f;
    public float bounceHeight = 0.3f;  // ƨ�� ����
    public float bounceSpeed = 6f;     // ƨ�� �ӵ�

    [Header("Visuals")]
    public SpriteRenderer spriteRenderer;
    public Transform gfxRoot;          // ��������Ʈ�� ���� ��Ʈ(������ �ڵ� �Ҵ�)

    private Rigidbody2D rb;
    private bool movingRight = true;
    private Transform player;

    private enum State { Patrol, Alert, Chase }
    private State state = State.Patrol;

    // �ߺ� Alert ����
    private bool isAlerting = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (gfxRoot == null)
        {
            // ��������Ʈ�� ������ �� Ʈ������, ������ �ڱ� �ڽ�
            gfxRoot = spriteRenderer != null ? spriteRenderer.transform : transform;
        }
    }

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void FixedUpdate()
    {
        bool playerInRange = IsPlayerInRange();

        // ���� ����
        switch (state)
        {
            case State.Patrol:
                if (playerInRange && !isAlerting)
                {
                    StartCoroutine(EnterAlert());
                }
                break;

            case State.Alert:
                // �ڷ�ƾ ���ο��� ���� ����
                break;

            case State.Chase:
                if (!playerInRange) state = State.Patrol;
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

        // �̵� ����
        rb.velocity = Vector2.zero;

        // �˸� ������ (gfxRoot �ڽ�)
        if (alertIconPrefab != null)
        {
            GameObject icon = Instantiate(alertIconPrefab, gfxRoot.position + alertOffset, Quaternion.identity, gfxRoot);
            Destroy(icon, alertDuration);
        }

        // ƨ��
        yield return StartCoroutine(BounceAnimation());

        // alertDuration ��ŭ ��� (Ƣ�� �ð��� ������ �߰� ���)
        yield return new WaitForSeconds(alertDuration);

        // ������ �÷��̾ ���� ���̸� �߰�, �ƴϸ� ����
        state = IsPlayerInRange() ? State.Chase : State.Patrol;
        isAlerting = false;
    }

    private IEnumerator BounceAnimation()
    {
        // gfxRoot �������� ƨ��
        float half = 0.5f / bounceSpeed;

        float elapsed = 0f;
        Vector3 start = gfxRoot.localPosition;
        Vector3 peak = start + Vector3.up * bounceHeight;

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

        // ��Ȯ�� ����
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

            case State.Chase:
                if (player != null)
                {
                    float dirX = Mathf.Sign(player.position.x - transform.position.x);
                    rb.velocity = new Vector2(dirX * chaseSpeed, rb.velocity.y);
                    movingRight = dirX > 0f;
                }
                break;
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
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        float r = detectRangeTiles * tileSize;
        Gizmos.DrawWireSphere(transform.position, r);
    }
}