using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Enemy Data (HpSO)")]
    [Tooltip("HpSO �ҷ�����")]
    public HpSO hpData;


    [Header("Damage Feedback")]
    [Tooltip("�ǰ� �� ����Ʈ")]
    public SpriteRenderer sr;


    private void Awake()
    {
        if (hpData != null)
        {
            hpData.ResetHealth();
        }

        if (sr == null)
            sr = GetComponentInChildren<SpriteRenderer>();
    }
    public void TakeDamage(float damage)
    {
        if (hpData == null) return;

        hpData.currentHealth -= damage;
        Debug.Log("�� HP: " + hpData.currentHealth);

        if (sr != null)
        {
            StopAllCoroutines();
            StartCoroutine(DamageEffect());
        }
        if (hpData.currentHealth <= 0f) 
        { 
            Die(); 
        }
    }
    System.Collections.IEnumerator DamageEffect() 
    {
     sr.color = hpData != null ? hpData.hitColor : Color.red; 
     yield return new WaitForSeconds(0.15f);
     sr.color = Color.white; 
    }

    void Die() 
    { 
    Debug.Log("�� ���!"); gameObject.SetActive(false); 
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerHitbox hitbox = collision.GetComponent<PlayerHitbox>();
        if (hitbox != null) 
        { 
            TakeDamage(hitbox.damage);
            Destroy(collision.gameObject); 
        } 
    }


}
