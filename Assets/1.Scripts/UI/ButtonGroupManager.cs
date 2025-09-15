using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonGroupManager : MonoBehaviour
{
    private List<ButtonScaler> buttons = new List<ButtonScaler>();
    private List<ButtonScaler> selectedButtons = new List<ButtonScaler>();

    [Header("선택된 버튼을 표시할 UI 슬롯")]
    public Image slot1Icon;
    public Image slot2Icon;

    [Header("팝업 Animator")]
    public UIPopupAnimator popupAnimator;

    public void RegisterButton(ButtonScaler button)
    {
        if (!buttons.Contains(button))
            buttons.Add(button);
    }

    public void OnButtonClicked(ButtonScaler clicked)
    {
        if (clicked.isSelected)
        {
            clicked.SetSelected(false);
            selectedButtons.Remove(clicked);
        }
        else
        {
            if (selectedButtons.Count < 2)
            {
                selectedButtons.Add(clicked);
                clicked.SetSelected(true);
            }
            else
            {
                ButtonScaler oldest = selectedButtons[0];
                oldest.SetSelected(false);
                selectedButtons.RemoveAt(0);

                selectedButtons.Add(clicked);
                clicked.SetSelected(true);
            }

            if (popupAnimator != null)
                popupAnimator.Hide();
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        for (int i = 0; i < selectedButtons.Count; i++)
        {
            if (i == 0)
            {
                slot1Icon.enabled = true;
                slot1Icon.sprite = selectedButtons[0].iconSprite;
            }
            else if (i == 1)
            {
                slot2Icon.enabled = true;
                slot2Icon.sprite = selectedButtons[1].iconSprite;
            }
        }
    }
}
