using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonGroupManager : MonoBehaviour
{
    private List<ButtonScaler> buttons = new List<ButtonScaler>();
    private ButtonScaler currentlyHovered = null;

    public void RegisterButton(ButtonScaler button)
    {
        if (!buttons.Contains(button))
            buttons.Add(button);
    }

    public void OnButtonHovered(ButtonScaler hovered)
    {
        currentlyHovered = hovered;
        foreach (var btn in buttons)
        {
            btn.AnimateScale(btn == hovered);
        }
    }

    public void OnButtonExit(ButtonScaler exited)
    {
        if (currentlyHovered == exited)
        {
            currentlyHovered = null;
            foreach (var btn in buttons)
            {
                btn.AnimateScale(false);
            }
        }
    }
}
