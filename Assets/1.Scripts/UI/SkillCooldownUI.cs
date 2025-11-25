using UnityEngine;
using UnityEngine.UI;

public class SkillCooldownUI : MonoBehaviour
{
    public PlayerSkillController controller;

    public KeyCode key;  // Q ¶Ç´Â E
    private PlayerElement element;

    public Image skillImage;
    public Image backImage;

    private SkillData lastSkill;

    void Update()
    {
        element = (key == KeyCode.Q)
            ? PlayerSO.Instance.currentElement_Q
            : PlayerSO.Instance.currentElement_E;

        SkillData skill = controller.GetSkillData(element, key);

        if (skill != lastSkill)
        {
            lastSkill = skill;

            if (skill != null)
            {
                skillImage.sprite = skill.skillIcon;
                backImage.sprite = skill.skillIcon;
            }
            else
                skillImage.sprite = null; 
        }

        float current = controller.GetCooldown(element, key);
        float max = controller.GetMaxCooldown(element, key);

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
