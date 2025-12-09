using System.Collections;
using System.Collections.Generic;
using System.IO.Enumeration;
using UnityEngine;


[CreateAssetMenu(fileName = "New Achievement Data", menuName = "Achievement/Achievement Data")]
public class AchievementData : ScriptableObject
{
    public string achievementName;          //업적 이름
    public string description;
    public AchievementType achievementType;
    public int requiredAmount;                  //필요 수량 (Ex : 코인 10개)
    public int rewardCoins;                     //보상 코인
    public bool isUnlocked;                     //달성 여부
    public Sprite icon;
}
