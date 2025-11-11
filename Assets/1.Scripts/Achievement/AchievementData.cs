using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AchievementType
{
    Story,
    Challenge,
    Collection,
    Skill,
    Exploration
}

[CreateAssetMenu(fileName = "New Achievement", menuName = "Achievement")]

public class AchievementData : ScriptableObject
{
    [Header("기본 세팅")]
    public string id; // 업적 고유 ID
    public string achievementName; // 업적 이름
    public string achievementDescription; // 업적 설명
    public AchievementType achievementType; // 업적 종류
    public bool isUnlocked; // 업적 달성 여부
    public int targetCount = 1;


}
