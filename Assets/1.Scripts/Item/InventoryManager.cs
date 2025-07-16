using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public InventorySlot[] inventorySlots;

    private void Awake()
    {
        Instance = this;
    }

    public bool AddItem(ItemData item)
    {
        foreach (InventorySlot slot in inventorySlots)
        {
            if (slot.IsEmpty)
            {
                slot.SetItem(item);
                return true;
            }
        }

        Debug.Log("인벤토리 공간이 부족합니다!");
        return false;
    }
}
