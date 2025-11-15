using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowBullet : MonoBehaviour
{
    [Header("세팅")]
    public float speed = 8f;      // 이동 속도
    public float lifeTime = 5f;   // 수명 (초 단위)

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
        // 현재 회전 기준으로 +X 방향으로 전진
        rb.velocity = (Vector2)transform.right * speed;

        // 5초 후 자동 삭제
        if (lifeTime > 0f) Destroy(gameObject, lifeTime);
    }

    public void Init(Vector2 dir, float newSpeed)
    {
        if (dir.sqrMagnitude > 0.0001f) dir.Normalize();
        rb.velocity = dir * newSpeed;

        // 회전도 맞추기
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
