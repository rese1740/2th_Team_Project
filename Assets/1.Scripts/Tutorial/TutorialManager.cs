using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("UI")]
    public GameObject tutorialPanel;
    public Text tutorialText;

    public GameObject[] tutorialGroups;

    private KeyCode requiredKey = KeyCode.Space;
    private bool allowMouseClick = true;
    private bool isActive = false;

    private void Awake()
    {
        Instance = this;
    }

   public void ShowTutorial(string message, int groupIndex, KeyCode key, bool allowClick)
    {
        isActive = true;
        requiredKey = key;
        allowMouseClick = allowClick;

        Time.timeScale = 0f;

        tutorialPanel.SetActive(true);
        tutorialText.text = message;

        for (int i = 0; i < tutorialGroups.Length; i++)
            tutorialGroups[i].SetActive(false);

        if (groupIndex >= 0 && groupIndex < tutorialGroups.Length)
            tutorialGroups[groupIndex].SetActive(true);

        Debug.Log("ShowTutorial ½ÇÇàµÊ");
    }

    private void Update()
    {
        if (!isActive) return;

        if (Input.GetKeyDown(requiredKey))
        {
            CloseTutorial();
            return;
        }

        if (allowMouseClick && Input.GetMouseButtonDown(0))
        {
            CloseTutorial();
            return;
        }
        

    }

    public void CloseTutorial()
    {
        isActive = false;


        tutorialPanel.SetActive(false);

  
        Time.timeScale = 1f;


        for (int i = 0; i < tutorialGroups.Length; i++)
            tutorialGroups[i].SetActive(false);
    }
}
