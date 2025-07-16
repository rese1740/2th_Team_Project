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

    [Tooltip("������ �̸�")]
    public string itemName;

    [Tooltip("������ ����")]
    [TextArea(3, 5)]
    public string itemDescription;

    [Tooltip("������ ������")]
    public Sprite itemIcon;

    [Tooltip("������ ����")]
    [Range(0, 10000)] public int itemPrice;

    [Tooltip("������ ȹ�� �� �÷��̾�� �ִ� ȿ�� ��ġ")]
    [Range(0, 100)] public float effectValue;

    [Tooltip("������ Ÿ��")]
    public ItemType itemType;

    public void Init()
    {
        Instance = this;
    }
}

