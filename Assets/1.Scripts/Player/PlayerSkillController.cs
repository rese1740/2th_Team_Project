using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillController : MonoBehaviour
{
    public ElementType currentElement;

    [SerializeField] private List<SkillData> skillList; // Inspector에서 등록
    private Dictionary<(ElementType, KeyCode), SkillData> skillMap;

    private void Awake()
    {
        skillMap = new Dictionary<(ElementType, KeyCode), SkillData>();

        foreach (var skill in skillList)
        {
            skillMap[(skill.elementType, skill.key)] = skill;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q)) UseSkill(KeyCode.Q);
        if (Input.GetKeyDown(KeyCode.E)) UseSkill(KeyCode.E);
    }

    private void UseSkill(KeyCode key)
    {
        if (skillMap.TryGetValue((currentElement, key), out SkillData skill))
        {
            Debug.Log($"사용한 스킬: {skill.skillName}");
            // Instantiate(skill.skillEffectPrefab, transform.position, Quaternion.identity);
            // 쿨타임 처리, 데미지 등등
        }
        else
        {
            Debug.LogWarning($"속성 {currentElement}에 해당하는 {key} 스킬이 없음");
        }
    }
}
