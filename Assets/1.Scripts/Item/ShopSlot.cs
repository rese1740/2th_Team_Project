using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopSlot : MonoBehaviour
{
    [Header("������ ����")]
    public ItemData item;

    public Image icon;
    public Text priceText;
    public Button buyButton;

    private void Start()
    {
        UpdateSlotUI();
    }

    public void UpdateSlotUI()
    {
        if (item == null)
        {
            Debug.LogWarning("�������� ������� ����!");
            icon.enabled = false;
            priceText.text = "-";
            return;
        }

        int currentLevel = PlayerSO.Instance.GetItemLevel(item.itemID);

        ItemLevelData levelData = item.levelStats.Find(l => l.level == currentLevel);

        if (levelData != null)
        {
            if (levelData.itemIcon != null)
                icon.sprite = levelData.itemIcon;

            priceText.text = levelData.itemPrice.ToString();
            icon.enabled = true;
        }
        else
        {
            Debug.LogWarning($"[{item.itemID}] {currentLevel}���� �����Ͱ� ����!");
            icon.enabled = false;
            priceText.text = "-";
        }
    }

    public void OnBuyButtonClicked()
    {
        if (item != null)
            ShopManager.Instance.BuyItem(item);
    }
}
