using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SkillType
{
    Move,
    Projectile,
    Buff,
    Summon  
}

[CreateAssetMenu(fileName = "NewSkillData", menuName = "ScriptableObjects/SkillData", order = 4)]
public class SkillData : ScriptableObject
{
    public string skillName;
    public KeyCode key; // Q or E
    public PlayerElement elementType;
    public SkillType skillType;
    public GameObject skillEffectPrefab;
    public float damage;
    public float cooldown;

    [Header("����ü ����")]
    public int projectileCount = 1;
    public float projectileSpeed = 10f;

    [Header("�̵� ����")]
    public float dashForce_Ground = 15f;   
    public float dashForce_Air = 18f;     
    public float dashDuration = 0.2f;
    public bool isDashing = false;
    public bool canDash = true;

}
