using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopSlot : MonoBehaviour
{
    [Header("아이템 설정")]
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
            Debug.LogWarning("아이템이 연결되지 않음!");
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
            Debug.LogWarning($"[{item.itemID}] {currentLevel}레벨 데이터가 없음!");
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
