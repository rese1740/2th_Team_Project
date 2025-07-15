using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSO", menuName = "PlayerSO", order = 1)]
public class PlayerSO : ScriptableObject
{
    public static PlayerSO Instance;

    [Header("Movement Settings")]
    [Range(0, 100)] public float moveSpeed = 5f;
    [Range(0, 100)] public float jumpForce = 10f;


    [Header("Stat Setting")]
    [Range(0, 100)] public float currentHealth = 100f;
    [Range(0, 100)] public float maxHealth = 100f;
    [Range(0, 100)] public float attackPower = 100f;



    public void Init()
    {
        Instance = this;
    }
}
