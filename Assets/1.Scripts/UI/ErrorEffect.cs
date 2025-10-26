using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ErrorEffect : MonoBehaviour
{
    [Header("에러 이미지들")]
    public GameObject[] errorImages;
    public GameObject restartBtn;
    public float interval = 0.1f;

    private void Awake()
    {
        StartCoroutine(ErrorSequence());
    }

    IEnumerator ErrorSequence()
    {
        foreach (var img in errorImages)
            img.SetActive(false);

        for (int i = 0; i < errorImages.Length; i++)
        {
            errorImages[i].SetActive(true);
            yield return new WaitForSeconds(interval);
        }

        if (restartBtn != null)
            restartBtn.SetActive(true);
    }
}