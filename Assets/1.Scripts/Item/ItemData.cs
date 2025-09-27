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

    [Header("�⺻ ����")]

    [Tooltip("������ �̸�")]
    public string itemID;

    [Tooltip("������ ����")]
    [TextArea(3, 5)]
    public string itemDescription;


    [Tooltip("������ Ÿ��")]
    public ItemType itemType;

    [Header("�ܰ躰 �ɷ�ġ")]
    public List<ItemLevelData> levelStats = new List<ItemLevelData>();

    public void Init()
    {
        Instance = this;
    }
}

[System.Serializable]
public class ItemLevelData
{
    [Tooltip("�ܰ�")]
    public int level;

    [Tooltip("������ ������")]
    public Sprite itemIcon;

    [Tooltip("������ ����")]
    [Range(0, 10000)] public int itemPrice;

    [Tooltip("�� �������� ������ �ִ� ȿ�� Ÿ��")]
    public EffectType effectType;

    [Tooltip("�ش� �ܰ迡�� �����ϴ� �ɷ�ġ ��ġ")]
    public float effectValue;
}
