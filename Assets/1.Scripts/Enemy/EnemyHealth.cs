using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyHealth : MonoBehaviour
{
    [Header("Enemy Data (HpSO ���ø�)")]
    public HpSO hpData;

    [Header("�ǰ� ����")]
    public SpriteRenderer sr;
    private bool isFrozen = false;

    [Header("��Ÿ�� ����(���� �ν��Ͻ�)")]
    public float maxHealth;
    public float currentHealth;
    private bool isDead = false;

    [Header("�ǰ� �ɼ�")]
    [Tooltip("���� Ʈ���� ���� ��Ʈ ������ ���� ª�� ���� �ð�(��). 0�̸� ��Ȱ��.")]
    public float invincibleTime = 0.1f;
    private bool invincible = false;

    [Header("�˹� ����")]
    [Tooltip("�ǰ� �� �ڷ� �и��� ���� ũ��")]
    public float knockbackForce = 10f;
    [Tooltip("�˹� �� �������� ��¦ Ƣ������� ��(0�̸� ����)")]
    public float knockbackUpForce = 2f;

    [Header("�̺�Ʈ")]
    public UnityEvent<float, float> onHealthChanged;
    public UnityEvent onDeath;

    [Header("HpUI")]
    [Tooltip("HpSlider �ҷ�����")]
    public Slider hpSlider;

    [Header("HpText")]
    [Tooltip("Hp���� ǥ��")]
    public TMP_Text hpText;


    private Coroutine flashRoutine;
    private Rigidbody2D rb;
    private Transform player;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (sr == null)
            sr = GetComponentInChildren<SpriteRenderer>();

        if (hpData != null)
            hpData.maxHealth = maxHealth ;
 
    }

    private void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        currentHealth = maxHealth;

        // �����̴� �ʱ� ����
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHealth;
            hpSlider.value = currentHealth;
        }
        // �ؽ�Ʈ ����
        if (hpText != null)
        {
            hpText.text = $"{currentHealth:0}/{maxHealth:0}";
        }
    }

    private void Update()
    {
        if (isFrozen)
            sr.color = Color.cyan;

        if (isDead) return;

        // �����̴� ����
        if (hpData != null && hpSlider != null)
        {
            hpSlider.maxValue = hpData.maxHealth;
            hpSlider.value = currentHealth;
        }

        // �ؽ�Ʈ ����
        if (hpText != null)
        {
            hpText.text = $"{currentHealth:0}/{maxHealth:0}";
        }
    }

    private void OnEnable()
    {
        isDead = false;
        invincible = false;

        currentHealth = (hpData != null) ? hpData.maxHealth : 1f;
        onHealthChanged?.Invoke(currentHealth, hpData != null ? hpData.maxHealth : 1f);

        if (sr != null) sr.color = Color.white;

        // ��� UI �ݿ�
        if (hpData != null && hpSlider != null)
        {
            hpSlider.maxValue = hpData.maxHealth;
            hpSlider.value = currentHealth;
        }

    }

    public void TakeDamage(float damage)
    {
        if (hpData == null || isDead) return;
        if (invincible) return;

        currentHealth -= damage;
        if (currentHealth < 0f) currentHealth = 0f;

        Debug.Log($"�� HP: {currentHealth}");

        if (PlayerSO.Instance != null)
            PlayerSO.Instance.rageValue += PlayerSO.Instance.rageGainRate;

        if (sr != null)
        {
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(DamageEffect());
        }

        ApplyKnockback();

        onHealthChanged?.Invoke(currentHealth, hpData.maxHealth);

        if (invincibleTime > 0f)
            StartCoroutine(CoInvincible(invincibleTime));

        if (currentHealth <= 0f)
            Die();
    }

    private void ApplyKnockback()
    {
        if (rb == null) return;

        Vector2 dir;
        if (player != null) dir = (transform.position - player.position).normalized;
        else dir = sr != null && sr.flipX ? Vector2.right : Vector2.left;

        rb.velocity = Vector2.zero;
        rb.AddForce(new Vector2(dir.x * knockbackForce, knockbackUpForce), ForceMode2D.Impulse);

        // BoundaryEnemy�� �˹� �˸���
        var be = GetComponent<BoundaryEnemy>();
        if (be != null)
            be.EnterKnockback(0.15f);
    }

    private IEnumerator DamageEffect()
    {
        Color hit = hpData != null ? hpData.hitColor : Color.red;
        sr.color = hit;
        yield return new WaitForSeconds(0.15f);
        if (!isDead) sr.color = Color.white;
        flashRoutine = null;
    }

    private IEnumerator CoInvincible(float time)
    {
        invincible = true;
        yield return new WaitForSeconds(time);
        invincible = false;
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        currentHealth = 0f;

        if (hpText != null)
            hpText.text = $"{currentHealth:0}/{maxHealth:0}";

        if (hpSlider != null)
            hpSlider.value = 0f;

        Debug.Log("�� ���!");
        if (hpData != null && PlayerSO.Instance != null)
            PlayerSO.Instance.Gold += hpData.gainGold;

        onDeath?.Invoke();
        gameObject.SetActive(false);

    }

    public void Freeze(float duration, float damage)
    {
        StartCoroutine(FreezeCoroutine(duration, damage));
    }

    private IEnumerator FreezeCoroutine(float duration, float damage)
    {
        if (isFrozen) yield break;
        isFrozen = true;
        TakeDamage(damage);

        Vector2 originalVelocity = Vector2.zero;

        if (rb != null)
        {
            originalVelocity = rb.velocity;
            rb.velocity = Vector2.zero;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        yield return new WaitForSeconds(duration);

        TakeDamage(damage);

        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.velocity = originalVelocity;
        }

        if (sr != null)
            sr.color = Color.white;

        isFrozen = false;
    }
}
