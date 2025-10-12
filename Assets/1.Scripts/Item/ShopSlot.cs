using UnityEngine;
using UnityEngine.UI;

public class ShopSlot : MonoBehaviour
{
    [Header("아이템 설정")]
    public ItemData item;

    public Image icon;
    public Text priceText;
    public Text itemDescriptionTxt;
    public Text countTxt;
    public Button buyButton;
    public bool isSold = false;
    private int currentlevel = 0;   

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

        if (item.itemType == ItemType.consumption)
        {
            // 소모 아이템은 레벨 없이 개수만 체크
            icon.sprite = item.itemIcon;
            icon.enabled = true;
            priceText.text = item.itemPrice.ToString();

            // 최대치 도달 시 버튼 비활성화
            int currentCount = PlayerSO.Instance.GetPotionCount(item.itemID);
            buyButton.interactable = currentCount < item.itemMaxCount;
            countTxt.text = $"({currentCount}/{item.itemMaxCount})";
        }
        else
        {
            // 레벨 있는 아이템 처리
            int currentLevel = PlayerSO.Instance.GetItemLevel(item.itemID);
            ItemLevelData levelData = item.levelStats.Find(l => l.level == currentLevel);
            countTxt.text = $"({currentlevel}/{item.itemMaxCount})";

            if (levelData != null && currentLevel < 3)
            {
                icon.sprite = levelData.itemIcon;
                icon.enabled = true;
                priceText.text = levelData.itemPrice.ToString();
                buyButton.interactable = true;
            }
            else
            {
                icon.enabled = false;
                priceText.text = "-";
                buyButton.interactable = false;
            }
        }
    }

    public void OnBuyButtonClicked()
    {
        if (item != null && !isSold)
        {
            if (ShopManager.Instance.BuyItem(item))
            {
                if (item.itemType == ItemType.consumption)
                {
                    bool canBuy = true;

                    switch (item.itemID)
                    {
                        case "HP_Potion":
                            if (PlayerSO.Instance.hpPotionCount < item.itemMaxCount) canBuy = true;
                            break;
                        case "RageDecrease_Potion":
                            if (PlayerSO.Instance.rageDecreasePotionCount < item.itemMaxCount) canBuy = true;
                            break;
                        case "RageIncrease_Potion":
                            if (PlayerSO.Instance.rageIncreasePotionCount < item.itemMaxCount) canBuy = true;
                            break;
                    }

                    if (!canBuy)
                    {
                        isSold = true;
                        buyButton.interactable = false;
                        UpdateSlotUI();
                    }
                }
                else
                {
                    isSold = true;
                    buyButton.interactable = false;
                    currentlevel++;
                    UpdateSlotUI();
                }
            }
        }
    }
}
