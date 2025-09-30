using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DissolveController : MonoBehaviour
{
    public Material dissovleMaterial;
    public float duration = 2f;

    // Start is called before the first frame update
    void Start()
    {
        dissovleMaterial = GetComponent<Renderer>().material;
        StartCoroutine(ChangeAmountOverTime());
    }

    IEnumerator ChangeAmountOverTime()
    {
        float elapsed = 0f;
        float startValue = -1f;
        float endValue = 1;

        while (elapsed <duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float value = Mathf.Lerp(startValue, endValue, t);
            dissovleMaterial.SetFloat("_Amout", value);
            yield return null;
        }

        dissovleMaterial.SetFloat("_Amount", endValue);               //이거 하다가 이펙트 적용 안 돼면 "Amount"로 고쳐야함 아님 이름 확인하기
    }

}
