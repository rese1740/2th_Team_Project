using System.Collections.Generic;
using UnityEngine;

public enum PlayerElement
{
    None,
    Fire,
    Water,
    Ice,
    Wind,
    Steam,
    Firestorm,
    Mud,
    Sandstorm,
    IceWater
}

[CreateAssetMenu(fileName = "PlayerSO", menuName = "PlayerSO", order = 1)]
public class PlayerSO : ScriptableObject
{
    private static PlayerSO _instance;
    public static PlayerSO Instance => _instance;

    private void OnEnable()
    {
        if (_instance == null)
            _instance = this;
    }

    [Header("Movement Settings")]
    [Range(0, 100)] public float moveSpeed = 5f;
    [Range(0, 100)] public float jumpForce = 10f;


    [Header("Stat Setting")]
    [Tooltip("�÷��̾� ���� ü��")]
    [Range(0, 100)] public float currentHealth = 100f;

    [Tooltip("�÷��̾� �ִ� ü��")]
    [Range(0, 500)] public float maxHealth = 100f;

    [Tooltip("�÷��̾� ���ݷ�")]
    [Range(0, 100)] public float attackPower = 100f;

    [Tooltip("�÷��̾� ũ��Ƽ�� Ȯ�� %")]
    [Range(0, 100)] public float critValue = 100f;

    [Tooltip("�÷��̾� ũ��Ƽ�� ������ ���� %")]
    [Range(0, 1000)] public float critPower = 100f;

    [Tooltip("�÷��̾� ���� ������")]
    [Range(0, 100)] public float rageValue = 50f;

    [Tooltip("�÷��̾� ���� �ð�")]
    [Range(0, 1000)] public float rageDuration = 10f;

    [Tooltip("�÷��̾� ���� ����� ����")]
    [Range(0, 1000)] public float rageAttack = 100f;

    [Tooltip("�÷��̾� ���� HP ���ҷ�")]
    [Range(0, 1000)] public float rageHPDecrease = 100f;

    [Tooltip("�÷��̾� ���� ������ ������")]
    [Range(0, 1000)] public float rageGainRate = 100f;



    [Tooltip("�÷��̾� ��差")]
    [Range(0, 1000)] public int Gold = 100;

    [Tooltip("�÷��̾� �Ӽ�")]
    public PlayerElement currentElement_Q = PlayerElement.None;
    public PlayerElement currentElement_E = PlayerElement.None;
    public PlayerElement saved1;
    public PlayerElement saved2;


    #region ������
    [Header("������ ����")]
    public Dictionary<string, int> itemLevels = new Dictionary<string, int>();

    [Header("�Ҹ� ������ ����")]
    public int hpPotionCount = 3;
    public int rageIncreasePotionCount = 3;
    public int rageDecreasePotionCount = 3;

    [SerializeField] private int maxPotionCount = 3; // �ִ� ���� ����

    public int GetItemLevel(string itemID)
    {
        return itemLevels.ContainsKey(itemID) ? itemLevels[itemID] : 0;
    }

    public void LevelUpItem(string itemID)
    {
        if (itemLevels.ContainsKey(itemID))
            itemLevels[itemID]++;
        else
            itemLevels[itemID] = 1;
    }

    public int GetPotionCount(string itemID)
    {
        if (itemID == "HpPotion")
        {
            return hpPotionCount;
        }
        else if (itemID == "RageIncreasePotion")
        {
            return rageIncreasePotionCount;
        }
        else if (itemID == "RageDecreasePotion")
        {
            return rageDecreasePotionCount;
        }
        return 0;
    }

    public void ApplyItemEffect(ItemData item, int currentLevel)
    {
        ItemLevelData levelData = item.levelStats.Find(l => l.level == currentLevel);

        if (levelData == null) return;

        switch (levelData.effectType)
        {
            case EffectType.MaxHP:
                maxHealth += levelData.effectValue;
                break;

            case EffectType.Speed:
                moveSpeed += levelData.effectValue;
                break;

            case EffectType.crit:
                critValue += levelData.effectValue;
                break;

            case EffectType.critValue:
                critPower += levelData.effectValue;
                break;

            case EffectType.Heal:
                currentHealth = Mathf.Min(maxHealth, currentHealth + levelData.effectValue);
                break;

            case EffectType.Rage_Increase:
                rageValue += levelData.effectValue;
                break;

            case EffectType.Rage_Decrease:
                rageValue = Mathf.Max(0, rageValue - levelData.effectValue);
                break;
        }
    }

    public void UsePotion(EffectType potionType)
    {
        switch (potionType)
        {
            case EffectType.Heal:
                if (hpPotionCount <= 0) return;
                currentHealth = Mathf.Min(maxHealth, currentHealth + 30f); // 30 ȸ��
                hpPotionCount--;
                break;

            case EffectType.Rage_Increase:
                if (rageIncreasePotionCount <= 0) return;
                rageValue = Mathf.Min(100f, rageValue + 30f); // Rage 30 ����
                rageIncreasePotionCount--;
                break;

            case EffectType.Rage_Decrease:
                if (rageDecreasePotionCount <= 0) return;
                rageValue = Mathf.Max(0f, rageValue - 30f); // Rage 30 ����
                rageDecreasePotionCount--;
                break;
        }
    }
}
#endregion

