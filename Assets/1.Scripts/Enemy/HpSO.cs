using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HpSO", menuName = "HpSO")]
public class HpSO : ScriptableObject
{
    [Header("Stats")]
    [Tooltip("Hp 값")]
    public float maxHealth = 100f;
    public int gainGold = 10;   

    [HideInInspector] public float currentHealth;

    [Header("Damage Feedback")]
    [Tooltip("피격 시 이펙트")]
    public Color hitColor = Color.red;

    // 게임 시작 시 초기화
    public void ResetHealth()
    {
        currentHealth = maxHealth;
    }
}

