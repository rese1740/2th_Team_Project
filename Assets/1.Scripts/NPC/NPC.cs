using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour
{
    public GameObject uiPanel;
    public void OpenWindow()
    {
        if (uiPanel != null)
        {
            UIStateManager.Instance.isUIOpen = true;
            uiPanel.SetActive(true);
        }
    }
}
