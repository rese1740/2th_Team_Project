using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopSlot : MonoBehaviour
{
    public Image icon;
    public Text priceText;
    public Button buyButton;
    private ItemData item;

    public void SetItem(ItemData newItem)
    {
        item = newItem;
        icon.sprite = item.itemIcon;
        priceText.text = item.itemPrice.ToString();
    }

    public void OnBuyButtonClicked()
    {
        ShopManager.Instance.BuyItem(item);
    }
}

