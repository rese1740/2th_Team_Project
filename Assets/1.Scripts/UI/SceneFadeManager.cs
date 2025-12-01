using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneFadeManager : MonoBehaviour
{
    public static SceneFadeManager Instance;

    [Header("Fade UI")]
    public Image fadeImage;           
    public float fadeDuration = 1f;    

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        StartCoroutine(FadeIn());
    }

    public void SceneMove(string sceneName)
    {

        StartCoroutine(FadeOutAndLoad(sceneName));
    }

    private IEnumerator FadeOutAndLoad(string sceneName)
    {
        yield return StartCoroutine(FadeOut());

        yield return SceneManager.LoadSceneAsync(sceneName);

        yield return StartCoroutine(FadeIn());
    }

    private IEnumerator FadeOut()
    {
        float time = 0f;
        Color color = fadeImage.color;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            color.a = Mathf.Lerp(0, 1, time / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }
    }

    private IEnumerator FadeIn()
    {
        float time = 0f;
        Color color = fadeImage.color;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            color.a = Mathf.Lerp(1, 0, time / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }
    }

}
