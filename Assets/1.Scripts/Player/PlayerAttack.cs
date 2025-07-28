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
        if (Input.GetKeyDown(KeyCode.Q))
        {
            playerData.currentHealth = playerData.maxHealth; // 테스트용: 체력 회복
            Debug.Log("플레이어 체력 회복: " + playerData.currentHealth);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            playerData.Gold += 100; // 테스트용: 골드 증가
            Debug.Log("플레이어 골드 증가: " + playerData.Gold);
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

    void SkillA()
    {

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
