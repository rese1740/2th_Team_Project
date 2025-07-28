using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSkillData", menuName = "ScriptableObjects/SkillData", order = 4)]
public class SkillData : ScriptableObject
{
    public string skillName;
    public KeyCode key; // Q or E
    public PlayerElement elementType;
    public GameObject skillEffectPrefab;
    public float damage;
    public float cooldown;
}
