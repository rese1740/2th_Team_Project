using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    consumption,
    Artifact
}

public enum EffectType
{
    Debuff,
    Speed,
    MaxHP,
    Arrow,
    crit,
    critValue,
    Heal,
    Rage_Increase,
    Rage_Decrease

}


[CreateAssetMenu(fileName = "ItemData", menuName = "ItemData", order = 3)]
public class ItemData : ScriptableObject
{
    public static ItemData Instance;

    [Header("기본 정보")]

    [Tooltip("아이템 이름")]
    public string itemID;

    [Tooltip("아이템 설명")]
    [TextArea(3, 5)]
    public string itemDescription;


    [Tooltip("아이템 타입")]
    public ItemType itemType;

    [Header("단계별 능력치")]
    public List<ItemLevelData> levelStats = new List<ItemLevelData>();

    public void Init()
    {
        Instance = this;
    }
}

[System.Serializable]
public class ItemLevelData
{
    [Tooltip("단계")]
    public int level;

    [Tooltip("아이템 아이콘")]
    public Sprite itemIcon;

    [Tooltip("아이템 가격")]
    [Range(0, 10000)] public int itemPrice;

    [Tooltip("이 레벨에서 영향을 주는 효과 타입")]
    public EffectType effectType;

    [Tooltip("해당 단계에서 증가하는 능력치 수치")]
    public float effectValue;
}
