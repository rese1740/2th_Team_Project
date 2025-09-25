using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerItem : MonoBehaviour
{
    public void GiveItemToPlayer(ItemData item)
    {
        int currentLevel = PlayerSO.Instance.GetItemLevel(item.itemID);

        PlayerSO.Instance.LevelUpItem(item.itemID);

        int newLevel = PlayerSO.Instance.GetItemLevel(item.itemID);
        PlayerSO.Instance.ApplyItemEffect(item, newLevel);

        Debug.Log($"{item.itemID} È¹µæ! ÇöÀç ·¹º§: {newLevel}");
    }
}
