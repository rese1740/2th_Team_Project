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
        iconImage.sprite = item.itemIcon;
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
