using System.Collections.Generic;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance;
    public List<AchievementData> allAchievements;
    private Dictionary<string, int> progress = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddProgress(AchievementType condition, int amount = 1)
    {
        foreach (var achievement in allAchievements)
        {
            if (achievement.achievementType != condition) continue;
            if (IsUnlocked(achievement.id)) continue;

            if (!progress.ContainsKey(achievement.id))
                progress[achievement.id] = 0;

            progress[achievement.id] += amount;

            if (progress[achievement.id] >= achievement.targetCount)
            {
                Unlock(achievement.id);
            }
        }
    }

    public void Unlock(string id)
    {
        if (IsUnlocked(id)) return;
        PlayerPrefs.SetInt($"Achievement_{id}", 1);
        PlayerPrefs.Save();
        Debug.Log($"¾÷Àû ÇØ±ÝµÊ: {id}");
    }

    public bool IsUnlocked(string id)
    {
        return PlayerPrefs.GetInt($"Achievement_{id}", 0) == 1;
    }
}
