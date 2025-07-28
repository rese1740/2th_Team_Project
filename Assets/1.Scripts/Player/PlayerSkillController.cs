using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillController : MonoBehaviour
{
    public PlayerElement currentElement;

    [Tooltip("플레이어 스킬 목록")]
    [SerializeField] private List<SkillData> skillList; 

    private Dictionary<(PlayerElement, KeyCode), SkillData> skillMap;
    private Dictionary<(PlayerElement, KeyCode), float> cooldownTimers = new();

    private void Awake()
    {
        skillMap = new Dictionary<(PlayerElement, KeyCode), SkillData>();

        foreach (var skill in skillList)
        {
            skillMap[(skill.elementType, skill.key)] = skill;
        }
    }

    private void Update()
    {
        HandleCooldowns();

        currentElement = PlayerSO.Instance.currentElement;
        if (Input.GetKeyDown(KeyCode.Q)) UseSkill(KeyCode.Q);
        if (Input.GetKeyDown(KeyCode.E)) UseSkill(KeyCode.E);
    }

    private void UseSkill(KeyCode key)
    {
        var skillKey = (currentElement, key);

        if (skillMap.TryGetValue(skillKey, out SkillData skill))
        {
            if (cooldownTimers.TryGetValue(skillKey, out float timeRemaining) && timeRemaining > 0f)
            {
                Debug.Log($"{skill.skillName}은 {timeRemaining:F1}초 남음");
                return;
            }

            Debug.Log($"사용한 스킬: {skill.skillName}");

            cooldownTimers[skillKey] = skill.cooldown;
        }
    }

    private void HandleCooldowns()
    {
        var keys = new List<(PlayerElement, KeyCode)>(cooldownTimers.Keys);

        foreach (var key in keys)
        {
            if (cooldownTimers[key] > 0f)
            {
                cooldownTimers[key] -= Time.deltaTime;

                if (cooldownTimers[key] < 0f)
                    cooldownTimers[key] = 0f;
            }
        }
    }
}
