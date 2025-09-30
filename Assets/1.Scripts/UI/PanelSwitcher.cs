using UnityEngine;
using UnityEngine.UI;

public class PanelSwitcher : MonoBehaviour
{
    public enum PanelType { Resolution, Sound }

    [Header("Root (����)")]
    [Tooltip("�ػ�/���� �г��� ���δ� �ֻ��� ������Ʈ(SettingPanel). ESC�� ������ �� �� ������Ʈ�� �Ѱ� ��.")]
    public GameObject container;

    [Header("Panels")]
    public GameObject resolutionPanel;
    public GameObject soundPanel;

    [Header("Tab Buttons (����)")]
    public Button resolutionTabButton;
    public Button soundTabButton;

    [Header("Initial")]
    public PanelType defaultPanel = PanelType.Resolution;
    [Tooltip("������ �� â�� ������� ����")]
    public bool openOnStart = false;

    // ���� ����
    private PanelType _current = PanelType.Resolution;
    private bool _isOpen = false;

    void Start()
    {
        // �� ��ư �̺�Ʈ ���� (���� ����)
        if (resolutionTabButton != null)
            resolutionTabButton.onClick.AddListener(() => ShowPanel(PanelType.Resolution));

        if (soundTabButton != null)
            soundTabButton.onClick.AddListener(() => ShowPanel(PanelType.Sound));


        if (openOnStart)
            Open(defaultPanel);
        else
            CloseAll();

        if (resolutionPanel == null) Debug.LogWarning("[PanelSwitcher] resolutionPanel�� ��� �ֽ��ϴ�.");
        if (soundPanel == null) Debug.LogWarning("[PanelSwitcher] soundPanel�� ��� �ֽ��ϴ�.");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Toggle();
        }
    }

    // ESC ���
    public void Toggle()
    {
        if (_isOpen) CloseAll();
        else Open(defaultPanel);
    }

    public void Open(PanelType panel)
    {
        if (container != null) container.SetActive(true);

        _isOpen = true;
        ShowPanel(panel);
    }

    public void ShowPanel(PanelType panel)
    {
        _current = panel;

        bool showResolution = (panel == PanelType.Resolution);

        if (resolutionPanel != null) resolutionPanel.SetActive(showResolution);

        if (soundPanel != null) soundPanel.SetActive(!showResolution);

        if (container == null)
        {
            _isOpen = (showResolution && resolutionPanel != null && resolutionPanel.activeSelf)
                      || (!showResolution && soundPanel != null && soundPanel.activeSelf);
        }
    }

    public void CloseAll()
    {
        if (container != null)
        {
            container.SetActive(false);
        }
        else
        {
            if (resolutionPanel != null) resolutionPanel.SetActive(false);
            if (soundPanel != null) soundPanel.SetActive(false);
        }
        _isOpen = false;
    }
}
