using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    private void Start()
    {
        Instance = this;
    }

    public bool TryBuy(ItemData item)
    {
        if (item == null) return false;

        if (item.itemType == ItemType.consumption)
        {
            int currentCount = PlayerSO.Instance.GetPotionCount(item.itemID);

            if (currentCount >= item.itemMaxCount)
            {
                Debug.LogWarning($"[{item.itemID}] 최대 개수 도달!");
                return false;
            }

            if (PlayerSO.Instance.Gold >= item.itemPrice)
            {
                PlayerSO.Instance.Gold -= item.itemPrice;
                return true;
            }
            else
            {
                Debug.Log("골드 부족!");
                return false;
            }
        }
        else
        {
            int currentLevel = PlayerSO.Instance.GetItemLevel(item.itemID);
            ItemLevelData levelData = item.levelStats.Find(l => l.level == currentLevel);

            if (levelData == null)
            {
                Debug.LogWarning($"[{item.itemID}] {currentLevel}레벨 데이터 없음!");
                return false;
            }

            if (PlayerSO.Instance.Gold >= levelData.itemPrice)
            {
                PlayerSO.Instance.Gold -= levelData.itemPrice;
                return true;
            }

            Debug.Log("골드 부족!");
            return false;
        }
    }

    public bool BuyItem(ItemData item)
    {
        if (TryBuy(item))
        {
            if (item.itemType == ItemType.consumption)
            {
                if (item.itemID == "HpPotion")
                {
                    PlayerSO.Instance.hpPotionCount++;
                }
                else if (item.itemID == "RageIncreasePotion")
                {
                    PlayerSO.Instance.rageIncreasePotionCount++;
                }
                else if (item.itemID == "RageDecreasePotion")
                {
                    PlayerSO.Instance.rageDecreasePotionCount++;
                }
                Debug.Log($"구매 완료: {item.itemID}, 현재 개수: {PlayerSO.Instance.GetPotionCount(item.itemID)}, 남은 골드: {PlayerSO.Instance.Gold}");
                return true;
            }
            else
            {
                PlayerSO.Instance.LevelUpItem(item.itemID);

                int newLevel = PlayerSO.Instance.GetItemLevel(item.itemID);
                PlayerSO.Instance.ApplyItemEffect(item, newLevel);

                Debug.Log($"구매 완료: {item.itemID}, 현재 레벨: {newLevel}, 남은 골드: {PlayerSO.Instance.Gold}");
                return true;
            }
        }
        else
        {
            Debug.Log("구매 실패: 골드 부족 또는 레벨 데이터 없음");
            return false;
        }
    }
}
