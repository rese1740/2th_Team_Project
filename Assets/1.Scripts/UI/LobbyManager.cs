using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    [Header("게임 방법 패널")]
    public GameObject helpPanel;

    [Header("게임 방법 페이지들")]
    public GameObject[] helpPages;

    private int currentPage = 0;
    private bool isHelpOpen = false;

    void Start()
    {
        if (helpPanel != null)
            helpPanel.SetActive(false);

        // 모든 페이지 비활성화
        DisableAllPages();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleHelp();
        }
    }


    public void QuitGame()
    {
        Debug.Log("게임 종료!");
        Application.Quit();
    }

  
    public void OpenHelp()
    {
        if (helpPanel == null) return;

        helpPanel.SetActive(true);
        Time.timeScale = 0f;
        isHelpOpen = true;

        currentPage = 0;
        ShowPage(currentPage);
    }


    public void CloseHelp()
    {
        if (helpPanel == null) return;

        helpPanel.SetActive(false);
        Time.timeScale = 1f;
        isHelpOpen = false;

        DisableAllPages();
    }


    public void ToggleHelp()
    {
        if (isHelpOpen) CloseHelp();
        else OpenHelp();
    }


    public void NextPage()
    {
        currentPage++;

        if (currentPage >= helpPages.Length)
        {
            CloseHelp(); 
            return;
        }

        ShowPage(currentPage);
    }

    public void PrevPage()
    {
        if (currentPage <= 0) return;

        currentPage--;
        ShowPage(currentPage);
    }

    private void ShowPage(int index)
    {
        DisableAllPages();

        if (index >= 0 && index < helpPages.Length)
            helpPages[index].SetActive(true);
    }

    private void DisableAllPages()
    {
        foreach (var page in helpPages)
            page.SetActive(false);
    }
}
