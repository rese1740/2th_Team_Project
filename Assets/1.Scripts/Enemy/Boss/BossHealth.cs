using UnityEngine;

public class BossHealth : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Damage Feedback")]
    public SpriteRenderer sr;

    private void Awake()
    {
        currentHealth = maxHealth;
        if (sr == null)
            sr = GetComponentInChildren<SpriteRenderer>();
    }

    // �÷��̾� ��Ʈ�ڽ��� �Ѿ˰� �浹���� �� ȣ��
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log("�� HP: " + currentHealth);

        if (sr != null)
        {
            StopAllCoroutines();
            StartCoroutine(DamageEffect());
        }

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    System.Collections.IEnumerator DamageEffect()
    {
        sr.color = Color.red;
        yield return new WaitForSeconds(0.15f);
        sr.color = Color.white;
    }

    void Die()
    {
        Debug.Log("�� ���!");
        // ���⿡ �׾��� ���� ó��(�ִϸ��̼�, ������Ʈ ��Ȱ��ȭ ��) �߰�
        gameObject.SetActive(false);
    }

    // �÷��̾� ��Ʈ�ڽ� �浹 ����
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // PlayerHitbox�� �浹�ߴ��� Ȯ��
        PlayerHitbox hitbox = collision.GetComponent<PlayerHitbox>();
        if (hitbox != null)
        {
            TakeDamage(hitbox.damage);
            // �ʿ��ϸ� ��Ʈ�ڽ� ����
            Destroy(collision.gameObject);
        }
    }
}
