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
    [Tooltip("HpSO 불러오기")]
    public HpSO hpData;

    [Header("HpUI")]
    [Tooltip("HpSlider 불러오기")]
    public Slider hpSlider;

    [Header("HpText")]
    [Tooltip("Hp숫자 표기")]
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

    // 플레이어 히트박스나 총알과 충돌했을 때 호출
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log("적 HP: " + currentHealth);

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
        Debug.Log("적 사망!");
        // 여기에 죽었을 때의 처리(애니메이션, 오브젝트 비활성화 등) 추가
        gameObject.SetActive(false);
    }

    // 플레이어 히트박스 충돌 감지
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // PlayerHitbox와 충돌했는지 확인
        IceProjectile hitbox = collision.GetComponent<IceProjectile>();
        if (hitbox != null)
        {
            TakeDamage(hitbox.damage);
            // 필요하면 히트박스 삭제
            Destroy(collision.gameObject);
        }
    }
}
