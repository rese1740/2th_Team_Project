using System.Collections;
using UnityEngine;

public class HomingProjectile : MonoBehaviour
{
    [Header("설정값")]
    public float speed = 10f;      
    public float rotateSpeed = 200f; 
    public float damage = 10;
    public float lifeTime = 5f;      
    public float launchDelay = 0.5f; 
    private bool isLaunched = false;

    private Transform target;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifeTime);

        if (launchDelay > 0f)
            StartCoroutine(LaunchAfterDelay());
        else
            isLaunched = true; // 즉시 발사
    }

    public void SetTarget(Transform newTarget, float damage, float speed)
    {
        this.target = newTarget;
        this.damage = damage;
        this.speed = speed;
    }
    IEnumerator LaunchAfterDelay()
    {
        yield return new WaitForSeconds(launchDelay);
        isLaunched = true;
    }

    private void FixedUpdate()
    {
        if (!isLaunched)
        {
            rb.velocity = Vector2.zero; 
            return;
        }

        if (target == null)
        {
            rb.velocity = transform.right * speed;
            return;
        }

        Vector2 direction = ((Vector2)target.position - rb.position).normalized;
        rb.velocity = direction * speed;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            EnemyHealth enemy = collision.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }

            Destroy(gameObject);
        }
    }
}
