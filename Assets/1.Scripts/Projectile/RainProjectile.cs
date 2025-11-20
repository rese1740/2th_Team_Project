using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RainProjectile : MonoBehaviour
{
    public float speed = 5f;
    public float lifetime = 2f;
    public float damage = 1f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.Translate(Vector3.down * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Enemy"))
        {
            EnemyHealth enemy = col.GetComponent<EnemyHealth>();
            if (enemy != null)
                enemy.TakeDamage(damage);

            Destroy(gameObject);
        }
    }
}
