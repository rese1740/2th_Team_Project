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
    public string animationName;
    public float damage;
    public float coolTime;

    [Header("����ü ����")]
    public int projectileCount = 1;
    public float projectileSpeed = 10f;

    [Header("�̵� ����")]
    public float dashingPower = 15f;   
    public float dashDuration = 0.2f;
    public bool isDashing = false;
    public bool canDash = true;

}
