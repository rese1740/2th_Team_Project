using DG.Tweening;
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
    private Tween currentTween;
    private Button button;
    private Image buttonImage;
    [HideInInspector] public bool isSelected = false;

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

    public void AnimateScale(bool enlarge)
    {
        currentTween?.Kill();

        if (enlarge)
            currentTween = transform.DOScale(originalScale * scaleUpSize, scaleDuration).SetEase(Ease.OutBack);
        else
            currentTween = transform.DOScale(originalScale, scaleDuration).SetEase(Ease.InBack);
    }

    public void OnButtonClicked()
    {
        groupManager.OnButtonClicked(this);
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        AnimateScale(isSelected);

        if (isSelected)
        {
            iconImg.enabled = true;
            iconImg.sprite = iconSprite;

            buttonImage.color = Color.gray;
            button.interactable = false;

            PlayerSO.Instance.currentElement = stateToSet;
        }
        else
        {
            iconImg.enabled = false;
            buttonImage.color = Color.white;
            button.interactable = true;
        }
    }
}
