using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillController : MonoBehaviour
{
    public PlayerElement currentElement_Q;
    public PlayerElement currentElement_E;

    [Tooltip("�÷��̾� ��ų ���")]
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

        currentElement_Q = PlayerSO.Instance.currentElement_Q;
        currentElement_E = PlayerSO.Instance.currentElement_E;
        if (Input.GetKeyDown(KeyCode.Q)) UseSkill(KeyCode.Q);
        if (Input.GetKeyDown(KeyCode.E)) UseSkill(KeyCode.E);
    }

    private void UseSkill(KeyCode key)
    {
        PlayerElement element = key switch
        {
            KeyCode.Q => currentElement_Q,
            KeyCode.E => currentElement_E,
            _ => PlayerElement.None
        };

        var skillKey = (element, key);

        if (skillMap.TryGetValue(skillKey, out SkillData skill))
        {
            if (cooldownTimers.TryGetValue(skillKey, out float timeRemaining) && timeRemaining > 0f)
            {
                Debug.Log($"{skill.skillName}�� {timeRemaining:F1}�� ����");
                return;
            }

            Debug.Log($"����� ��ų: {skill.skillName}");

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
