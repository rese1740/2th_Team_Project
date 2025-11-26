using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHitBox : MonoBehaviour
{
    public float damage;
    public float duration = 0.2f; 
    public bool isPiercing = false;

    private void Start()
    {
        Destroy(gameObject, duration);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
              
        if (other.CompareTag("Enemy"))
        {
            EnemyHealth enemy = other.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                Debug.Log("적 피격");
                enemy.TakeDamage(damage);

                if(!isPiercing)
                    Destroy(gameObject);
            }

            //추가
            BossTwinHp twin = other.GetComponent<BossTwinHp>();
            if (twin != null)
            {
                Debug.Log("보스 피격");
                twin.TakeDamage(damage);

                if (!isPiercing)
                    Destroy(gameObject);
            }
        }
    }
}
