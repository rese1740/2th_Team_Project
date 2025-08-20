using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BELBullet : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 8f;      // �̵� �ӵ�
    public float lifeTime = 5f;   // ���� (�� ����)

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.drag = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    void OnEnable()
    {
        // ���� ȸ�� �������� +X �������� ����
        rb.velocity = (Vector2)transform.right * speed;

        // 5�� �� �ڵ� ����
        if (lifeTime > 0f) Destroy(gameObject, lifeTime);
    }

    /// <summary>
    /// BoundaryEnemyL���� ����� �ӵ��� ���� �����ϰ� ���� �� ȣ��
    /// </summary>
    public void Init(Vector2 dir, float newSpeed)
    {
        if (dir.sqrMagnitude > 0.0001f) dir.Normalize();
        rb.velocity = dir * newSpeed;

        // ȸ���� ���߱�
        float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, ang);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}