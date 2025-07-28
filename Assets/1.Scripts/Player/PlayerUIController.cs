using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class PlayerUIController : MonoBehaviour
{
    [Header("Gameobject")]
    public UIPopupAnimator elementPanel;

    private void Update()
    {
        if (UIStateManager.Instance.isUIOpen) return;

        if (Input.GetKeyDown(KeyCode.V))
        {
            elementPanel.Show();
            UIStateManager.Instance.isUIOpen = true;
        }

    }

}
