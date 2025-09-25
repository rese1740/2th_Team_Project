using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Image iconImage;
    private ItemData currentItem;

    public bool IsEmpty => currentItem == null;

    public void SetItem(ItemData item)
    {
        currentItem = item;
        int currentLevel = PlayerSO.Instance.GetItemLevel(item.itemID);
        ItemLevelData levelData = item.levelStats.Find(l => l.level == currentLevel);
        if (levelData != null && levelData.itemIcon != null)
        {
            iconImage.sprite = levelData.itemIcon;
            iconImage.enabled = true;
        }
        iconImage.enabled = true;
    }

    public void Clear()
    {
        currentItem = null;
        iconImage.sprite = null;
        iconImage.enabled = false;
    }

    public ItemData GetItem()
    {
        return currentItem;
    }
}
