using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Playables;
using UnityEngine.UI;

public class ButtonScaler : MonoBehaviour
{
    public UIPopupAnimator popupAnimator;
    public Image iconImg;
    public Sprite iconSprite;
    public PlayerElement stateToSet;
    public float scaleUpSize = 1.2f;
    public float scaleDuration = 0.2f;

    private Vector3 originalScale;
    private Tween currentTween;
    private ButtonGroupManager groupManager;

    void Awake()
    {
        originalScale = transform.localScale;
        iconImg.enabled = false;
        groupManager = GetComponentInParent<ButtonGroupManager>();
        groupManager.RegisterButton(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        groupManager.OnButtonHovered(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        groupManager.OnButtonExit(this);
    }

    public void AnimateScale(bool enlarge)
    {
        currentTween?.Kill();

        if (enlarge)
        {
            currentTween = transform.DOScale(originalScale * scaleUpSize, scaleDuration).SetEase(Ease.OutBack);
        }
        else
        {
            currentTween = transform.DOScale(originalScale, scaleDuration).SetEase(Ease.InBack);
        }
    }

    public void OnButtonClicked()
    {
        PlayerSO.Instance.currentElement = stateToSet;
        iconImg.enabled = true;
        iconImg.sprite = iconSprite;
        popupAnimator.Hide();
        UIStateManager.Instance.isUIOpen = false;
    }
}
