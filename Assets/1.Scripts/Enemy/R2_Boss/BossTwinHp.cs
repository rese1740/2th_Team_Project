using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


[RequireComponent(typeof(Rigidbody2D))]
public class BossTwinHp : MonoBehaviour
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
    public float invincibleTime = 0.1f;
    private bool invincible = false;

    [Header("넉백 설정")]
    public float knockbackForce = 10f;
    public float knockbackUpForce = 2f;

    [Header("이벤트")]
    public UnityEvent<float, float> onHealthChanged;
    public UnityEvent onDeath;
    public string sceneName;
    public bool isBoss = false;

    private Coroutine flashRoutine;
    private Rigidbody2D rb;
    private Transform player;

    [Header("HPUI")]
    public Slider hpSlider;

    [Header("HPText")]
    public TMP_Text hpText;
  
    [Header("Heal Mode")]
    [Tooltip("맞으면 체력 회복되는 모드")]
    public bool isHealMode = false;
    [Tooltip("힐 모드일 때, 맞은 데미지 * 값 만큼 회복")]
    public float healOnHitAmount = 1.5f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (sr == null)
            sr = GetComponentInChildren<SpriteRenderer>();

        if (hpData != null)
        {
            maxHealth = hpData.maxHealth;
        }
    }

    private void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        currentHealth = maxHealth;

        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHealth;
            hpSlider.value = currentHealth;
        }

        if (hpText != null)
        {
            hpText.text = $"{currentHealth:0}/{maxHealth:0}";
        }
    }

    private void Update()
    {
        if (isFrozen)
        {
            sr.color = Color.cyan;
        }

        if (isDead) return;

        if (hpData != null && hpSlider != null)
        {
            hpSlider.maxValue = hpData.maxHealth;
            hpSlider.value = currentHealth;
        }

        if (hpText != null)
        {
            hpText.text = $"{currentHealth:0}/{maxHealth:0}";
        }
    }

    private void OnEnable()
    {
        isDead = false;
        invincible = false;

        currentHealth = maxHealth;
        onHealthChanged?.Invoke(currentHealth, maxHealth);

        if (sr != null) sr.color = Color.white;

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

        //힐 모드
        if (isHealMode)
        {
            float healAmount = damage * healOnHitAmount;
            currentHealth += healAmount;
            if (currentHealth > maxHealth) currentHealth = maxHealth;

            Debug.Log($"[HealMode] 공격을 맞고 회복 +{healAmount} => {currentHealth}");

            onHealthChanged?.Invoke(currentHealth, maxHealth);

            if (sr != null)
            {
                if (flashRoutine != null) StopCoroutine(flashRoutine);
                flashRoutine = StartCoroutine(DamageEffect());
            }

            return;
        }

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
        if (hpData != null && !PlayerSO.Instance.isRaging)
            PlayerSO.Instance.Gold += hpData.gainGold;
        else
            PlayerSO.Instance.dataPiece += hpData.dataPiece;

        onDeath?.Invoke();
        gameObject.SetActive(false);
    }

    public void Freeze(float duration, float damage)
    {
        StartCoroutine(FreezeCoroutine(duration, damage));
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
