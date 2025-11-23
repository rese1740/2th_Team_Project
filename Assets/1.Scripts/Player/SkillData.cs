using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SkillType
{
    Move,
    Projectile,
    Buff,
    Summon,
    Targeting,
    Enchant,
}

[CreateAssetMenu(fileName = "NewSkillData", menuName = "ScriptableObjects/SkillData", order = 4)]
public class SkillData : ScriptableObject
{
    public string skillName;
    public Sprite skillIcon;
    public KeyCode key; // Q or E
    public PlayerElement elementType;
    public SkillType skillType;
    public GameObject skillEffectPrefab;
    public string animationName;
    public float damage;
    public float coolTime;

    [Header("투사체 세팅")]
    public int projectileCount = 1;
    public float projectileSpeed = 10f;

    [Header("타겟팅 세팅")]
    public float range = 1;
    public LayerMask targetLayer;

    [Header("인첸트 세팅")]
    public int attackCount;
    public float enhancedBonusDamage;

    [Header("이동 세팅")]
    public float dashingPower = 15f;   
    public float dashDuration = 0.2f;
    public bool useGhostEffect;
}
