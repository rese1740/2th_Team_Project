using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillController : MonoBehaviour
{
    public ElementType currentElement;

    [SerializeField] private List<SkillData> skillList; // Inspector���� ���
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
            Debug.Log($"����� ��ų: {skill.skillName}");
            // Instantiate(skill.skillEffectPrefab, transform.position, Quaternion.identity);
            // ��Ÿ�� ó��, ������ ���
        }
        else
        {
            Debug.LogWarning($"�Ӽ� {currentElement}�� �ش��ϴ� {key} ��ų�� ����");
        }
    }
}
