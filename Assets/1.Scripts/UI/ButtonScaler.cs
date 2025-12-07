using UnityEngine;
using UnityEngine.UI;

public class ButtonScaler : MonoBehaviour
{
    public UIPopupAnimator popupAnimator; 
    public Sprite iconSprite;            
    public PlayerElement stateToSet;  
    public float scaleUpSize = 1.2f;
    public float scaleDuration = 0.2f;

    public bool isLocked = false;

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

        groupManager = GetComponentInParent<ButtonGroupManager>();
        groupManager.RegisterButton(this);

        if (isLocked)
        {
            button.interactable = false;
            buttonImage.color = new Color(0.5f, 0.5f, 0.5f); 
        }

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

        if (isLocked)
        {
            buttonImage.color = Color.gray;
            button.interactable = false;
            return;  
        }

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
        this.buttonImage.color = Color.gray;

        if (groupManager != null)
        {
            groupManager.selectedCount++;

            if (groupManager.selectedCount >= 2 && groupManager.popupAnimator != null)
            {
                groupManager.UpdatePlayerElement();
                PlayerSO.Instance.currentElement_Q = PlayerSO.Instance.saved2;
                groupManager.popupAnimator.Hide();
            }
        }
    }
    public void Unlock()
    {
        isLocked = false;
        button.interactable = true;
        buttonImage.color = Color.white;
    }
    public void ApplyLockState()
    {
        if (isLocked)
        {
            button.interactable = false;
            buttonImage.color = new Color(0.5f, 0.5f, 0.5f);
        }
        else
        {
            button.interactable = true;
            buttonImage.color = Color.white;
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
            buttonImage.color = Color.white;
            button.interactable = true;
        }
        else
        {
            buttonImage.color = Color.gray;
            button.interactable = false;
        }
    }
}
