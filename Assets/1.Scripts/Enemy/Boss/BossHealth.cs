using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossHealth : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth ;
    private float currentHealth;

    [Header("Damage Feedback")]
    public SpriteRenderer sr;

    [Header("HP Data (HpSO)")]
    [Tooltip("HpSO �ҷ�����")]
    public HpSO hpData;

    [Header("HpUI")]
    [Tooltip("HpSlider �ҷ�����")]
    public Slider hpSlider;

    [Header("HpText")]
    [Tooltip("Hp���� ǥ��")]
    bool updateText = true;
    public TMP_Text hpText;


    private void Awake()
    {
        currentHealth = maxHealth;

        if (sr == null)
            sr = GetComponentInChildren<SpriteRenderer>();

        if (hpData != null && hpSlider != null)
        {
            hpSlider.maxValue = maxHealth;
            hpSlider.value = currentHealth;
        }
    }
    private void Update()
    {
        if (hpData == null || hpSlider == null) return;

        if (updateText && hpText != null)
        {
            hpText.text = $"{currentHealth}/{maxHealth}";
        }
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
        IceProjectile hitbox = collision.GetComponent<IceProjectile>();
        if (hitbox != null)
        {
            TakeDamage(hitbox.damage);
            // �ʿ��ϸ� ��Ʈ�ڽ� ����
            Destroy(collision.gameObject);
        }
    }
}
