using UnityEngine;

public enum PlayerElement
{
    None,
    Fire,
    Water,
    Earth,
    Wind,
    Steam,
    Firestorm,
    Mud,
    Sandstorm
}

[CreateAssetMenu(fileName = "PlayerSO", menuName = "PlayerSO", order = 1)]
public class PlayerSO : ScriptableObject
{
    public static PlayerSO Instance;

    [Header("Movement Settings")]
    [Range(0, 100)] public float moveSpeed = 5f;
    [Range(0, 100)] public float jumpForce = 10f;


    [Header("Stat Setting")]
    [Tooltip("�÷��̾� ���� ü��")]
    [Range(0, 100)] public float currentHealth = 100f;
    
    [Tooltip("�÷��̾� �ִ� ü��")]
    [Range(0, 100)] public float maxHealth = 100f;

    [Tooltip("�÷��̾� ���� ������")]
    [Range(0, 100)] public float rageValue = 50f;

    [Tooltip("�÷��̾� ���ݷ�")]
    [Range(0, 100)] public float attackPower = 100f;

    [Tooltip("�÷��̾� ũ��Ƽ�� Ȯ�� %")]
    [Range(0, 100)] public float critValue = 100f;

    [Tooltip("�÷��̾� ũ��Ƽ�� ������ ���� %")]
    [Range(0, 1000)] public float critPower = 100f;

    [Tooltip("�÷��̾� ��差")]
    [Range(0, 1000)] public int Gold = 100;

    [Tooltip("�÷��̾� �Ӽ�")]
    public PlayerElement currentElement_Q = PlayerElement.None;
    public PlayerElement currentElement_E = PlayerElement.None;
    public PlayerElement saved1;
    public PlayerElement saved2;

    public void Init()
    {
        Instance = this;
        Debug.Log("�ƴ���?");
    }
}
