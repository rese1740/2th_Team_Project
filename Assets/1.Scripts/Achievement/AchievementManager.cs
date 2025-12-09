using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager instance;

    [Header("Achievement Settings")]
    public List<AchievementData> allachievements = new List<AchievementData>();

    [Header("UI References")]
    public GameObject achievementPopupPrefab;
    public Transform popupParent;
    public GameObject achievementPanel;
    public Transform achievementListContent;
    public GameObject ahievementSlotPrefab;

    private Dictionary<AchievementType, int> progressData = new Dictionary<AchievementType, int>();           //통계 저장

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        ResetAllAchievements();                   //시작시에 리셋 강제로 (테스트용)
        foreach (AchievementType type in System.Enum.GetValues(typeof(AchievementType))) 
        {
          progressData[type] = 0;
        }
        LoadAchievements();
        UpdateAchievementUI();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //업적 UI 업데이트
    public void UpdateAchievementUI()
    {
        if (achievementListContent == null || ahievementSlotPrefab == null)
            return;

        //기존 슬롯 제거
        foreach (Transform child in achievementListContent)
        {
            Destroy(child.gameObject);
        }

        foreach(AchievementData achievement in allachievements)
        {
            GameObject slot = Instantiate(ahievementSlotPrefab, achievementListContent);
            AchievementSlot slotScript = slot.GetComponent<AchievementSlot>();
            if (slotScript != null)
            {
                slotScript.SetAchievement(achievement, GetProgress(achievement));

            }
        }
    }

    public void UpdateProgress(AchievementType type, int amount = 1)        //업적 진행도 업데이트
    {
        progressData[type] += amount;
        foreach (AchievementData achievement in allachievements)
        {
            if (achievement.achievementType == type && !achievement.isUnlocked)
            {
                if (progressData[type] >= achievement.requiredAmount)
                {
                   UnlockAchievement(achievement);

                }
            }
        }
       
    }

    void UnlockAchievement(AchievementData achievement)        //업적 달성 처리
    {
        achievement.isUnlocked = true;
       
        SaveAchievements();
        UpdateAchievementUI();
       
    }

    void ShowAchievementPopup(AchievementData ahievement)
    {
        if (achievementPopupPrefab != null && popupParent != null)
        {
            GameObject popup = Instantiate(achievementPopupPrefab, popupParent);
            
            Text titleText = popup.transform.Find("Title")?.GetComponent<Text>();
            Text descrText = popup.transform.Find("Description")?.GetComponent<Text>();


            if (titleText != null)
                titleText.text = "업적 달성! : ";
            if (descrText != null)
                descrText.text = ahievement.achievementName;

            Destroy(popup, 3.0f);
        }
    }

    public float GetProgress(AchievementData achievement)       //진행도 가져오기
    {
        if (achievement.isUnlocked) return 1f;
        int current = progressData.ContainsKey(achievement.achievementType) ? progressData[achievement.achievementType] : 0;
        return Mathf.Min((float)current / achievement.requiredAmount, 1f);

    }

    void SaveAchievements()
    {
        foreach (var kvp in progressData)
        {
            PlayerPrefs.SetInt("Achihevement_" + kvp.Key, kvp.Value);
        }

        foreach(AchievementData achievement in allachievements)
        {
            PlayerPrefs.SetInt("Unlocked_" + achievement.achievementName, achievement.isUnlocked ? 1 : 0);
        }

        PlayerPrefs.Save();
    }

    void LoadAchievements() //업적 로드 함수
    {
        foreach (AchievementType type in System.Enum.GetValues(typeof(AchievementType)))
        {
            progressData[type] = PlayerPrefs.GetInt("Achihevement_" + type, 0);
        }

        foreach (AchievementData achievement in allachievements)
        {
            achievement.isUnlocked = PlayerPrefs.GetInt("Unlocked_" + achievement.achievementName, 0) == 1;
        }
        
            
        
    }

    public void ResetAllAchievements()
    {
        foreach (AchievementType type in System.Enum.GetValues(typeof(AchievementType)))
        {
            progressData[type] = 0;
            PlayerPrefs.DeleteKey("Achihevement_" + type);
        }

        foreach (AchievementData achievement in allachievements)
        {
            achievement.isUnlocked = false;
            PlayerPrefs.DeleteKey("Unlocked_" + achievement.achievementName);
        }

        PlayerPrefs.Save();
        UpdateAchievementUI();

    }
}
