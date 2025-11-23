using UnityEngine;

public class GhostSprite : MonoBehaviour
{
    [Header("Sprite 설정")]
    public SpriteRenderer sr;

    [Header("폭발 데미지 설정")]
    public float radius = 1.5f;
    public int damage = 10;
    public LayerMask enemyLayer;
    public GameObject explosionVFX;

    public void Init(Sprite sprite, bool flipX, Vector3 scale)
    {
        sr.sprite = sprite;
        sr.flipX = flipX;
        transform.localScale = scale;
    }

    private void OnDestroy()
    {
        // 폭발 VFX 출력
        if (explosionVFX != null)
            Instantiate(explosionVFX, transform.position, Quaternion.identity);

        // 범위 내 적 찾기
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, enemyLayer);

        foreach (var hit in hits)
        {
            EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
