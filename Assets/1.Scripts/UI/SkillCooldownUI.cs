using UnityEngine;
using UnityEngine.UI;

public class SkillCooldownUI : MonoBehaviour
{
    public PlayerSkillController controller;

    public KeyCode key;  // Q 또는 E
    private PlayerElement element;

    public Image skillImage;
    public Image backImage;

    private SkillData lastSkill;

    void Update()
    {
        // 현재 슬롯의 element 자동 업데이트
        element = (key == KeyCode.Q)
            ? PlayerSO.Instance.currentElement_Q
            : PlayerSO.Instance.currentElement_E;

        // 현재 스킬 가져오기
        SkillData skill = controller.GetSkillData(element, key);

        // 스킬이 바뀌었으면 아이콘 변경
        if (skill != lastSkill)
        {
            lastSkill = skill;

            if (skill != null)
            {
                skillImage.sprite = skill.skillIcon;
                backImage.sprite = skill.skillIcon;
            }
            else
                skillImage.sprite = null; // None 일 때
        }

        // 쿨타임 정보
        float current = controller.GetCooldown(element, key);
        float max = controller.GetMaxCooldown(element, key);

        // 쿨타임 fillAmount (1 = 가림, 0 = 보여짐)
        if (max <= 0f)
        {
            skillImage.fillAmount = 0f;
        }
        else
        {
            skillImage.fillAmount = current / max;
        }
    }
}
