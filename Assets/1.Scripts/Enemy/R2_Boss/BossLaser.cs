using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossLaser : MonoBehaviour
{
    public SpriteRenderer sr;
    public BoxCollider2D hitCollider;

    private float telegraphDuration;
    private float damage;
    private float tickInterval;
    private float activeDuration;
    private LayerMask playerLayer;

    private bool isActive = false;

    private void Awake()
    {
        if (!hitCollider) hitCollider = GetComponent<BoxCollider2D>();
        if (!sr) sr = GetComponent<SpriteRenderer>();

        hitCollider.isTrigger = true;
        hitCollider.enabled = false;
    }

    public void Setup(float telegraphDuration, float damage, float tickInterval, float activeDuration, LayerMask playerLayer)
    {
        this.telegraphDuration = telegraphDuration;
        this.damage = damage;
        this.tickInterval = tickInterval;
        this.activeDuration = activeDuration;
        this.playerLayer = playerLayer;

        StartCoroutine(CoRun());
    }

    private IEnumerator CoRun()
    {
        // 1) 텔레그래프: 반투명 선, 데미지 없음
        if (sr != null)
        {
            Color c = sr.color;
            c.a = 0.6f;
            sr.color = c;
        }

        yield return new WaitForSeconds(telegraphDuration);

        // 2) 레이저 ON: 불투명 + 콜라이더 활성
        if (sr != null)
        {
            Color c = sr.color;
            c.a = 1f;
            sr.color = c;
        }

        hitCollider.enabled = true;
        isActive = true;

        float elapsed = 0f;
        while (elapsed < activeDuration)
        {
            DoDamageTick();
            yield return new WaitForSeconds(tickInterval);
            elapsed += tickInterval;
        }

        isActive = false;
        hitCollider.enabled = false;

        Destroy(gameObject);
    }

    private void DoDamageTick()
    {
        if (!isActive || hitCollider == null) return;

        Bounds b = hitCollider.bounds;
        Collider2D[] hits = Physics2D.OverlapBoxAll(b.center, b.size, 0f, playerLayer);
        if (hits == null || hits.Length == 0) return;

        foreach (var h in hits)
        {
            var playerHp = h.GetComponent<PlayerAttack>();
            if (playerHp != null)
            {
                playerHp.TakeDamage(damage);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (hitCollider == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(hitCollider.bounds.center, hitCollider.bounds.size);
    }
}
