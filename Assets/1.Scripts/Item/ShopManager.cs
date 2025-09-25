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
            Debug.LogWarning($"[{item.itemID}] {currentLevel}���� ������ ����!");
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

            Debug.Log($"���� �Ϸ�: {item.itemID}, ���� ����: {newLevel}, ���� ���: {PlayerSO.Instance.Gold}");
        }
        else
        {
            Debug.Log("���� ����: ��� ���� �Ǵ� ���� ������ ����");
        }
    }
}
