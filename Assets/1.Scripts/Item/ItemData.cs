using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    consumption,
    Artifact
}

public enum ElementType
{
    None,
    Fire,
    Water,
    Electric,
    Wind,
}

[CreateAssetMenu(fileName = "ItemData", menuName = "ItemData", order = 3)]
public class ItemData : ScriptableObject
{
    public static ItemData Instance;

    [Header("Item Settings")]

    [Tooltip("아이템 이름")]
    public string itemName;

    [Tooltip("아이템 설명")]
    [TextArea(3, 5)]
    public string itemDescription;

    [Tooltip("아이템 아이콘")]
    public Sprite itemIcon;

    [Tooltip("아이템 가격")]
    [Range(0, 10000)] public int itemPrice;

    [Tooltip("아이템 획득 시 플레이어에게 주는 효과 수치")]
    [Range(0, 100)] public float effectValue;

    [Tooltip("아이템 타입")]
    public ItemType itemType;

    public void Init()
    {
        Instance = this;
    }
}

