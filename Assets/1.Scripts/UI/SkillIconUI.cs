using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SkillIconUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string skillDescription;
    public void OnPointerEnter(PointerEventData eventData)
    {
        ToolTip.Instance.Show(skillDescription, eventData.position);
    }
        
    public void OnPointerExit(PointerEventData eventData)
    {
        ToolTip.Instance.Hide();
    }


}
