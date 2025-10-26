using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyHealth : MonoBehaviour
{
    [Header("Enemy Data (HpSO ���ø�)")]
    public HpSO hpData;

    [Header("�ǰ� ����")]
    public SpriteRenderer sr;

    [Header("��Ÿ�� ����(���� �ν��Ͻ�)")]
    [SerializeField] private float currentHealth;
    [SerializeField] private bool isDead = false;

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

    private Coroutine flashRoutine;
    private Rigidbody2D rb;
    private Transform player;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (sr == null)
            sr = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    private void OnEnable()
    {
        isDead = false;
        invincible = false;

        currentHealth = (hpData != null) ? hpData.maxHealth : 1f;
        onHealthChanged?.Invoke(currentHealth, hpData != null ? hpData.maxHealth : 1f);

        if (sr != null) sr.color = Color.white;
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

        //�˹� ó��
        ApplyKnockback();

        onHealthChanged?.Invoke(currentHealth, hpData != null ? hpData.maxHealth : 1f);

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

        //BoundaryEnemy�� �˹� �˸��� (��: 0.15�� ����)
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

        Debug.Log("�� ���!");
        if (hpData != null && PlayerSO.Instance != null)
            PlayerSO.Instance.Gold += hpData.gainGold;

        onDeath?.Invoke();
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var hitbox = collision.GetComponent<PlayerHitbox>();
        if (hitbox != null)
        {
            TakeDamage(hitbox.damage);
            Destroy(collision.gameObject);
        }
    }
}
