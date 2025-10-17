using UnityEngine;

[CreateAssetMenu(fileName = "ElementSO", menuName = "Player/ElementSO")]
public class ElementSO : ScriptableObject
{
    [Header("�Ӽ� ����")]
    public PlayerElement elementType;

    [Header("����/��� ����")]
    public float attackPower = 10f;
    public float maxHP = 10f;
    public float moveSpeed = 1f;
    public float rageValue = 1f;
    public float critChance = 5f;
    public float critPower = 1.5f;

    [Header("���� ����")]
    [Tooltip("�÷��̾� ���� ����� ����")]
    [Range(0, 1000)] public float rageAttack = 100f;

    [Tooltip("�÷��̾� ���� HP ���ҷ�")]
    [Range(0, 1000)] public float rageHPDecrease = 100f;

    [Tooltip("�÷��̾� ���� ������ ������")]
    [Range(0, 1000)] public float rageGainRate = 100f;
}
