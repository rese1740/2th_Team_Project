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
            uiPanel.SetActive(true);
        }
    }
}
