using System.Collections.Generic;
using UnityEngine;


public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;
    public ShopSlot[] slots;
    public List<ItemData> itemsForSale;

    void Start()
    {
        Instance = this;

        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].SetItem(itemsForSale[i]);
        }
    }
    public bool TryBuy(ItemData item)
    {
        if (PlayerSO.Instance.Gold >= item.itemPrice)
        {
          //  bool added = InventoryManager.Instance.AddItem(item);
           // if (!added) return false;

            PlayerSO.Instance.Gold -= item.itemPrice;
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
            Debug.Log($"구매 완료: {item.itemID}, 현재 레벨: {newLevel}, 남은 골드: {PlayerSO.Instance.Gold}");
        }
        else
        {
            Debug.Log("구매 실패: 골드 부족");
        }
    }
}
