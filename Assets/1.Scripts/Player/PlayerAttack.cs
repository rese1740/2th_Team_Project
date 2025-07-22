using System.Collections;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Components")]
    public PlayerSO playerData;
    SpriteRenderer sr;

    [Header("Settings")]
    public Transform attackPoint;
    public GameObject hitboxPrefab;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (UIStateManager.Instance.isUIOpen) return;
        if (Input.GetMouseButtonDown(0))
        {
            Attack();
        }
    }

    void Attack()
    {
        GameObject hitboxInstance = Instantiate(hitboxPrefab, attackPoint.position, attackPoint.rotation);

        PlayerHitbox hitbox = hitboxInstance.GetComponent<PlayerHitbox>();
        if (hitbox != null)
        {
            float finalDamage = playerData.attackPower;

            // 크리티컬 판정
            float rand = Random.Range(0f, 100f);
            if (rand < playerData.critValue)
            {
                finalDamage *= playerData.critPower / 100f;
                Debug.Log("💥 크리티컬 히트! 데미지: " + finalDamage);
            }

            hitbox.damage = finalDamage;
        }
    }

    public void TakeDamage(float damage)
    {
        playerData.currentHealth -= damage;

        StartCoroutine(DamageEffect());

        if (playerData.currentHealth <= 0)
        {
            Die();
        }
    }

    IEnumerator DamageEffect()
    {
        sr.color = Color.red;

        yield return new WaitForSeconds(0.2f);

        sr.color = Color.white;
    }

    void Die()
    {
        Debug.Log("플레이어 사망");
    }
}
