using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HpSO", menuName = "HpSO")]
public class HpSO : ScriptableObject
{
    [Header("Stats")]
    [Tooltip("Hp ��")]
    public float maxHealth = 100f;
    public int gainGold = 10;   

    [HideInInspector] public float currentHealth;

    [Header("Damage Feedback")]
    [Tooltip("�ǰ� �� ����Ʈ")]
    public Color hitColor = Color.red;

    // ���� ���� �� �ʱ�ȭ
    public void ResetHealth()
    {
        currentHealth = maxHealth;
    }
}

