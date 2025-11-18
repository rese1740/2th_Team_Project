using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ToolTip : MonoBehaviour
{
    public static ToolTip Instance;
    public GameObject tooltipPanel;
    public Text tooltipText;

    private void Awake()
    {
        Instance = this;
        Hide();
    }

    public void Show(string text, Vector3 position)
    {
        tooltipText.text = text;
        tooltipPanel.SetActive(true);

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, position);

        RectTransform rt = tooltipPanel.GetComponent<RectTransform>();
        rt.anchoredPosition = screenPoint + new Vector2(-400, -300); 
    }

    public void Hide()
    {
        tooltipPanel.SetActive(false);
    }
}
