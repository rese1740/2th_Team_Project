using Cinemachine;
using System.Collections;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("컴퍼넌트")]
    public PlayerSO playerData;

    Animator animator;

    [Header("공격 세팅")]
    public Transform attackPoint;
    public GameObject hitboxPrefab1;
    public GameObject hitboxPrefab2;

    [Header("폭주 세팅")]
    public bool isRaging = false;
    private float originalHealth;

    [Header("콤보 세팅")]
    public int maxCombo = 3;
    public float comboResetTime = 1f;   // 콤보 초기화 시간
    public float attackDelay = 0.3f;    // 공격 텀
    private int currentCombo = 0;
    private float lastAttackTime = 0f;
    private bool isAttacking = false;

    [Header("사망 세팅")]
    bool isDie = false;
    public GameObject diePanel;
    public CinemachineVirtualCamera virtualCamera;
    public float zoomAmount = 3f;     
    public float zoomDuration = 0.5f;
    private float originalSize;

  

    private void Start()
    {
        animator = GetComponent<Animator>();
        playerData.currentHealth = playerData.maxHealth;
    }

    private void Update()
    {
        if (UIStateManager.Instance.isUIOpen) return;

        if (!isRaging && playerData.rageValue >= 100f)
        {
            StartRage();
        }

        #region 포션 사용
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            PlayerSO.Instance.UsePotion(EffectType.Heal);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            PlayerSO.Instance.UsePotion(EffectType.Rage_Increase);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            PlayerSO.Instance.UsePotion(EffectType.Rage_Decrease);
        }

        #endregion

        if (Input.GetMouseButtonDown(0))
        {
            TryAttack();
        }
        if(Input.GetKeyDown(KeyCode.I))
        {
            PlayerSO.Instance.infiniteHealth = !PlayerSO.Instance.infiniteHealth;
        }
    }


    void StartRage()
    {
        Debug.Log("폭주 모드 시작");
        isRaging = true;
        playerData.rageValue = 100f;
        playerData.attackPower += playerData.rageAttack;
        originalHealth = playerData.currentHealth;
        playerData.currentHealth -= playerData.rageHPDecrease;

        StartCoroutine(RageRoutine());
    }

    IEnumerator RageRoutine()
    {
        float timer = 0f;

        while (timer < playerData.rageDuration)
        {
            playerData.rageValue = Mathf.Max(0, playerData.rageValue - (Time.deltaTime * playerData.rageDuration));

            timer += Time.deltaTime;
            yield return null;
        }

        EndRage();
    }

    public void EndRage()
    {
        Debug.Log("폭주 모드 종료");
        isRaging = false;
        playerData.attackPower -= playerData.rageAttack;
        playerData.currentHealth = originalHealth;
        playerData.rageValue = 0f;
    }



    void TryAttack()
    {
        if (isAttacking) return;

        if (Time.time - lastAttackTime > comboResetTime)
        {
            currentCombo = 0;
        }

        currentCombo++;
        lastAttackTime = Time.time;

        string element = PlayerSO.Instance.currentElement_Q.ToString(); // ex) "Fire"
        string comboName = currentCombo <= 2 ? "attack1" : "attack2";

        animator.SetTrigger($"{element}_{comboName}");

        if (currentCombo > maxCombo)
            currentCombo = 1;

        StartCoroutine(AttackCooldown());
    }

    IEnumerator AttackCooldown()
    {
        isAttacking = true;
        yield return new WaitForSeconds(attackDelay);
        isAttacking = false;
    }

    public void SpawnHitbox1()
    {
        GameObject hitboxInstance = Instantiate(hitboxPrefab1, attackPoint.position, attackPoint.rotation);
        PlayerHitBox hitbox = hitboxInstance.GetComponent<PlayerHitBox>();

        if (hitbox != null)
        {
            float finalDamage = playerData.attackPower;

            float rand = Random.Range(0f, 100f);
            if (rand < playerData.critValue)
            {
                finalDamage *= playerData.critPower / 100f;
                Debug.Log("크리티컬 히트! 데미지: " + finalDamage);
            }

            finalDamage *= 1f + (currentCombo - 1) * 0.2f;

            hitbox.damage = finalDamage;
        }
    }

    public void SpawnHitbox2()
    {
        GameObject hitboxInstance = Instantiate(hitboxPrefab2, attackPoint.position, attackPoint.rotation);
        PlayerHitBox hitbox = hitboxInstance.GetComponent<PlayerHitBox>();

        if (hitbox != null)
        {
            float finalDamage = playerData.attackPower;

            float rand = Random.Range(0f, 100f);
            if (rand < playerData.critValue)
            {
                finalDamage *= playerData.critPower / 100f;
                Debug.Log("크리티컬 히트! 데미지: " + finalDamage);
            }

            finalDamage *= 1f + (currentCombo - 1) * 0.2f;

            hitbox.damage = finalDamage;
        }
    }

    private IEnumerator ZoomCoroutine()
    {
        float elapsed = 0f;
        float startSize = virtualCamera.m_Lens.OrthographicSize;
        float targetSize = originalSize - zoomAmount;

        Vector3 originalOffset = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_TrackedObjectOffset;

        virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_TrackedObjectOffset = Vector3.zero;

        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;
            virtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(startSize, targetSize, elapsed / zoomDuration);
            yield return null;
        }

        virtualCamera.m_Lens.OrthographicSize = targetSize;
        diePanel.SetActive(true);

        virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_TrackedObjectOffset = originalOffset;
    }

    public void TakeDamage(float damage)
    {
        if (PlayerSO.Instance.infiniteHealth || isDie) return;

        playerData.currentHealth -= damage;

        if (playerData.currentHealth <= 0 && !isDie)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("플레이어 사망");
        animator.SetTrigger("die");
        UIStateManager.Instance.isUIOpen = true;
        isDie = true;
         StartCoroutine(ZoomCoroutine());

        #region 이동 제어
        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        rb.velocity = Vector2.zero;     
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        #endregion
    }
}
