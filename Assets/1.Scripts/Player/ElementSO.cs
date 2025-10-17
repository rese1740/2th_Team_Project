using UnityEngine;

[CreateAssetMenu(fileName = "ElementSO", menuName = "Player/ElementSO")]
public class ElementSO : ScriptableObject
{
    [Header("속성 정보")]
    public PlayerElement elementType;

    [Header("공격/방어 스탯")]
    public float attackPower = 10f;
    public float maxHP = 10f;
    public float moveSpeed = 1f;
    public float rageValue = 1f;
    public float critChance = 5f;
    public float critPower = 1.5f;

    [Header("폭주 관련")]
    [Tooltip("플레이어 폭주 공결력 증가")]
    [Range(0, 1000)] public float rageAttack = 100f;

    [Tooltip("플레이어 폭주 HP 감소량")]
    [Range(0, 1000)] public float rageHPDecrease = 100f;

    [Tooltip("플레이어 폭주 게이지 증가량")]
    [Range(0, 1000)] public float rageGainRate = 100f;
}
