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

        yield return new WaitForSeconds(0.2f); // �����̴� �ð�

        sr.color = Color.white; // ���� �� (�Ǵ� ������ �� ��)
    }

    void Die()
    {
        Debug.Log("�÷��̾� ���");
    }
}
