using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyHealth : MonoBehaviour
{
    [Header("Enemy Data (HpSO 템플릿)")]
    public HpSO hpData;

    [Header("피격 연출")]
    public SpriteRenderer sr;
    private bool isFrozen = false;

    [Header("런타임 상태(개별 인스턴스)")]
    public float maxHealth;
    public float currentHealth;
    private bool isDead = false;

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

    [Header("HpUI")]
    [Tooltip("HpSlider 불러오기")]
    public Slider hpSlider;

    [Header("HpText")]
    [Tooltip("Hp숫자 표기")]
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

        // 슬라이더 초기 세팅
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHealth;
            hpSlider.value = currentHealth;
        }
        // 텍스트 갱신
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

        // 슬라이더 갱신
        if (hpData != null && hpSlider != null)
        {
            hpSlider.maxValue = hpData.maxHealth;
            hpSlider.value = currentHealth;
        }

        // 텍스트 갱신
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

        // 즉시 UI 반영
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

        Debug.Log($"적 HP: {currentHealth}");

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

        // BoundaryEnemy에 넉백 알리기
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

        Debug.Log("적 사망!");
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
