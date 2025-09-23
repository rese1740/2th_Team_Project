using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class BDShockwave : MonoBehaviour
{
    [SerializeField] private float startRadius = 0.1f;
    [SerializeField] private float endRadius = 3.5f;
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private bool destroyAfter = true;

    private CircleCollider2D col;

    private void Awake()
    {
        col = GetComponent<CircleCollider2D>();
        col.isTrigger = true;
    }

    private void OnEnable()
    {
        StartCoroutine(CoExpand());
    }

    private IEnumerator CoExpand()
    {
        float t = 0f;
        while (t < duration)
        {
            float r = Mathf.Lerp(startRadius, endRadius, t / duration);
            col.radius = r;
            t += Time.deltaTime;
            yield return null;
        }
        col.radius = endRadius;

        if (destroyAfter) Destroy(gameObject, 0.05f);
    }
}
