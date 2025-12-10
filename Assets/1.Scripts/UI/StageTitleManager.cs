using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageTitleManager : MonoBehaviour
{
    public static StageTitleManager Instance;

    [Header("Stage Title UI Panel (Image)")]
    public GameObject titlePanel;        
    public Image titleImage;             
    public Sprite[] stageSprites;          

    public float showDuration = 2f;       

    private void Awake()
    {
        Instance = this;
        titlePanel.SetActive(false);
    }

    public void ShowStageTitle(int stageNumber)
    {
        // 방어 체크
        if (stageNumber < 0 || stageNumber >= stageSprites.Length)
        {
            Debug.LogWarning("Stage sprite not found!");
            return;
        }

        titleImage.sprite = stageSprites[stageNumber];

        titlePanel.SetActive(true);
        StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(showDuration);
        titlePanel.SetActive(false);
    }

}

