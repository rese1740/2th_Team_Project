using System.Collections;
using UnityEngine;

public class UIPopupAnimator : MonoBehaviour
{
    public float duration = 0.3f;
    public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public bool beginHiden = true;

    private Vector3 originalScale;

    void Awake()
    {
        originalScale = transform.localScale;

        if (beginHiden)
        {
            gameObject.SetActive(false);
            transform.localScale = Vector3.zero;
        }
    }

    public void Show()
    {
        StopAllCoroutines();
        gameObject.SetActive(true);
        StartCoroutine(ScaleRoutine(Vector3.zero, originalScale));
    }

    public void Hide()
    {
        StopAllCoroutines();
        StartCoroutine(ScaleRoutine(transform.localScale, Vector3.zero, () => gameObject.SetActive(false)));
        UIStateManager.Instance.isUIOpen = false;
    }

    private IEnumerator ScaleRoutine(Vector3 from, Vector3 to, System.Action onComplete = null)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = easeCurve.Evaluate(elapsed / duration);
            transform.localScale = Vector3.LerpUnclamped(from, to, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = to;
        onComplete?.Invoke();
    }
}
