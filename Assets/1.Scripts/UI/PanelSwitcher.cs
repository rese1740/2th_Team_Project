using UnityEngine;
using UnityEngine.UI;

public class PanelSwitcher : MonoBehaviour
{
    [Header("Panels")]
    public GameObject resolutionPanel;
    public GameObject soundPanel;

    [Header("Tab Button")]
    public Button resolutionTabButton;
    public Button soundTabButton;

    [Header("Initial display panel")]
    public string defaultPanel = "Resolution";

    void Start()
    {
        // ��ư�� ������ Ŭ�� �̺�Ʈ ����
        if (resolutionTabButton != null)
            resolutionTabButton.onClick.AddListener(() => ShowPanel("Resolution"));
        if (soundTabButton != null)
            soundTabButton.onClick.AddListener(() => ShowPanel("Sound"));

        // �ʱ� �г� ǥ��
        ShowPanel(defaultPanel);
    }

    public void ShowPanel(string name)
    {
        bool showResolution = name == "Resolution";
        if (resolutionPanel != null) resolutionPanel.SetActive(showResolution);
        if (soundPanel != null) soundPanel.SetActive(!showResolution);

    }
}