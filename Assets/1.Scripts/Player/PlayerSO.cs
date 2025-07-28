using UnityEngine;

public enum PlayerElement
{
    None = 0,
    Fire = 1,
    Water = 2,
    Earth = 3,
    Air = 4
}

[CreateAssetMenu(fileName = "PlayerSO", menuName = "PlayerSO", order = 1)]
public class PlayerSO : ScriptableObject
{
    public static PlayerSO Instance;

    [Header("Movement Settings")]
    [Range(0, 100)] public float moveSpeed = 5f;
    [Range(0, 100)] public float jumpForce = 10f;


    [Header("Stat Setting")]
    [Tooltip("플레이어 현재 체력")]
    [Range(0, 100)] public float currentHealth = 100f;
    
    [Tooltip("플레이어 최대 체력")]
    [Range(0, 100)] public float maxHealth = 100f;

    [Tooltip("플레이어 폭주 게이지")]
    [Range(0, 100)] public float rageValue = 50f;

    [Tooltip("플레이어 공격력")]
    [Range(0, 100)] public float attackPower = 100f;

    [Tooltip("플레이어 크리티컬 확률 %")]
    [Range(0, 100)] public float critValue = 100f;

    [Tooltip("플레이어 크리티컬 데미지 증가 %")]
    [Range(0, 1000)] public float critPower = 100f;

    [Tooltip("플레이어 골드량")]
    [Range(0, 1000)] public int Gold = 100;

    [Tooltip("플레이어 속성")]
    public PlayerElement currentElement = PlayerElement.None;



    public void Init()
    {
        Instance = this;
    }
}
