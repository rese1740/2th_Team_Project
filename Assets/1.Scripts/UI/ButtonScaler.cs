using UnityEngine;
using UnityEngine.UI;

public class ButtonScaler : MonoBehaviour
{
    public UIPopupAnimator popupAnimator; // 버튼 클릭 시 닫을 팝업
    public Image iconImg;                 // 버튼 아이콘
    public Sprite iconSprite;             // 버튼 고유 아이콘
    public PlayerElement stateToSet;      // 선택 속성
    public float scaleUpSize = 1.2f;
    public float scaleDuration = 0.2f;

    private Vector3 originalScale;
    private Button button;
    private Image buttonImage;
    public bool isSelected = false;
    public bool isUse = false;
    public ButtonScaler otherButton;
    private ButtonGroupManager groupManager;

    void Awake()
    {
        originalScale = transform.localScale;
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
        iconImg.enabled = false;

        groupManager = GetComponentInParent<ButtonGroupManager>();
        groupManager.RegisterButton(this);
       
    }

    private void Update()
    {
        if (isUse)
        {
            SetSelected();
        }
    }
    public void ForceSelect()
    {
        if (isSelected) return;

        otherButton.isSelected = true;
        isSelected = true;

        if (groupManager.selectedCount == 0)
        {
            PlayerSO.Instance.saved1 = stateToSet;
        }
        else if (groupManager.selectedCount == 1)
        {
            PlayerSO.Instance.saved2 = stateToSet;
        }

        if (groupManager != null)
        {
            groupManager.selectedCount++;

            if (groupManager.selectedCount >= 2 && groupManager.popupAnimator != null)
            {
                groupManager.UpdatePlayerElement();
                groupManager.popupAnimator.Hide();
            }
        }
    }


    public void OnButtonClicked()
    {
        groupManager.OnButtonClicked(this);
    }

    public void SetSelected()
    {
        if (isSelected)
        {
            iconImg.enabled = true;
            iconImg.sprite = iconSprite;

            buttonImage.color = Color.white;
            button.interactable = true;
        }
        else
        {
            iconImg.enabled = false;
            buttonImage.color = Color.gray;
            button.interactable = false;
        }
    }
}
