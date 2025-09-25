using System.Collections.Generic;
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

        return false;
    }

    public void BuyItem(ItemData item)
    {
        if (TryBuy(item))
        {
            PlayerSO.Instance.LevelUpItem(item.itemID);

            int newLevel = PlayerSO.Instance.GetItemLevel(item.itemID);
            PlayerSO.Instance.ApplyItemEffect(item, newLevel);

            Debug.Log($"구매 완료: {item.itemID}, 현재 레벨: {newLevel}, 남은 골드: {PlayerSO.Instance.Gold}");
        }
        else
        {
            Debug.Log("구매 실패: 골드 부족 또는 레벨 데이터 없음");
        }
    }
}
