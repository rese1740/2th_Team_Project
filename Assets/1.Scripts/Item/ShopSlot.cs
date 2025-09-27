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
    public Text itemDescriptionTxt;
    public Button buyButton;
    private bool isSold = false;

    private void Start()
    {
        UpdateSlotUI();
    }

    public void UpdateSlotUI()
    {
        if (item == null || isSold)  
        {
            icon.enabled = false;
            priceText.text = "-";
            buyButton.interactable = false; 
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
            buyButton.interactable = true;
        }
        else
        {
            icon.enabled = false;
            priceText.text = "-";
         
        }
    }

    public void OnBuyButtonClicked()
    {
        if (item != null && !isSold) 
        {
            if (ShopManager.Instance.BuyItem(item))
            {
                isSold = true;
                buyButton.interactable = false;
            }
        }
    }
}
