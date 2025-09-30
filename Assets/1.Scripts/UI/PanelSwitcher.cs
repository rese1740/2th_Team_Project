using UnityEngine;
using UnityEngine.UI;

public class PanelSwitcher : MonoBehaviour
{
    public enum PanelType { Resolution, Sound }

    [Header("Root (선택)")]
    [Tooltip("해상도/사운드 패널을 감싸는 최상위 오브젝트(SettingPanel). ESC로 여닫을 때 이 오브젝트를 켜고 끔.")]
    public GameObject container;

    [Header("Panels")]
    public GameObject resolutionPanel;
    public GameObject soundPanel;

    [Header("Tab Buttons (선택)")]
    public Button resolutionTabButton;
    public Button soundTabButton;

    [Header("Initial")]
    public PanelType defaultPanel = PanelType.Resolution;
    [Tooltip("시작할 때 창을 열어둘지 여부")]
    public bool openOnStart = false;

    // 내부 상태
    private PanelType _current = PanelType.Resolution;
    private bool _isOpen = false;

    void Start()
    {
        // 탭 버튼 이벤트 연결 (있을 때만)
        if (resolutionTabButton != null)
            resolutionTabButton.onClick.AddListener(() => ShowPanel(PanelType.Resolution));

        if (soundTabButton != null)
            soundTabButton.onClick.AddListener(() => ShowPanel(PanelType.Sound));


        if (openOnStart)
            Open(defaultPanel);
        else
            CloseAll();

        if (resolutionPanel == null) Debug.LogWarning("[PanelSwitcher] resolutionPanel이 비어 있습니다.");
        if (soundPanel == null) Debug.LogWarning("[PanelSwitcher] soundPanel이 비어 있습니다.");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Toggle();
        }
    }

    // ESC 토글
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
