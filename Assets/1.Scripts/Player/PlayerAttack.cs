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
            hitbox.damage = playerData.attackPower;
        }
    }

    public void TakeDamage(float damage)
    {
        playerData.currentHealth -= damage;

        StartCoroutine(HitEffect());

        if (playerData.currentHealth <= 0)
        {
            Die();
        }
    }

    IEnumerator HitEffect()
    {
        sr.color = Color.red;

        yield return new WaitForSeconds(0.2f); // 깜빡이는 시간

        sr.color = Color.white; // 원래 색 (또는 저장해 둔 색)
    }

    void Die()
    {
        Debug.Log("플레이어 사망");
    }
}
