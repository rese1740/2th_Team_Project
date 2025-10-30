using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyHealth : MonoBehaviour
{
    [Header("Enemy Data (HpSO 템플릿)")]
    public HpSO hpData;

    [Header("피격 연출")]
    public SpriteRenderer sr;
    private bool isFrozen = false;

    [Header("런타임 상태(개별 인스턴스)")]
    [SerializeField] private float currentHealth;
    [SerializeField] private bool isDead = false;

    [Header("피격 옵션")]
    [Tooltip("연속 트리거 다중 히트 방지를 위한 짧은 무적 시간(초). 0이면 비활성.")]
    public float invincibleTime = 0.1f;
    private bool invincible = false;

    [Header("넉백 설정")]
    [Tooltip("피격 시 뒤로 밀리는 힘의 크기")]
    public float knockbackForce = 10f;
    [Tooltip("넉백 후 수직으로 살짝 튀어오르는 값(0이면 없음)")]
    public float knockbackUpForce = 2f;

    [Header("이벤트")]
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

        if(hpData != null)
        {
            hpData.ResetHealth();
        }
    }

    private void Update()
    {
        if(isFrozen)
        {
          sr.color = Color.cyan;
        }
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

        Debug.Log($"적 HP: {currentHealth}");

        if (PlayerSO.Instance != null)
            PlayerSO.Instance.rageValue += PlayerSO.Instance.rageGainRate;

        if (sr != null)
        {
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(DamageEffect());
        }

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

        //BoundaryEnemy에 넉백 알리기 (예: 0.15초 정도)
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

        Debug.Log("적 사망!");
        if (hpData != null && PlayerSO.Instance != null)
            PlayerSO.Instance.Gold += hpData.gainGold;

        onDeath?.Invoke();
        gameObject.SetActive(false);
    }

    public void Freeze(float duration,float damage)
    {
        StartCoroutine(FreezeCoroutine(duration,damage));
    }

    IEnumerator FreezeCoroutine(float duration, float d)
    {
        if (isFrozen) yield break;
        isFrozen = true;
        TakeDamage(d);

        Vector2 originalVelocity = Vector2.zero;

        if (rb != null)
        {
            originalVelocity = rb.velocity;
            rb.velocity = Vector2.zero;
            rb.constraints = RigidbodyConstraints2D.FreezeAll; 
        }

        yield return new WaitForSeconds(duration);

        TakeDamage(d);

        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.None; 
            rb.velocity = originalVelocity; 
        }

        if (sr != null)
            sr.color = Color.white;

        isFrozen = false;
    }

}
