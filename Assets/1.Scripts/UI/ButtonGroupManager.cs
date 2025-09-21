using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonGroupManager : MonoBehaviour
{
    private List<ButtonScaler> buttons = new List<ButtonScaler>();
    public List<ButtonScaler> selectedButtons = new List<ButtonScaler>();

    [Header("선택된 버튼을 표시할 UI 슬롯")]
    public Image slot1Icon;
    public Image slot2Icon;
    public UIPopupAnimator popupAnimator;
   
    public int selectedCount = 0;

    private Dictionary<(PlayerElement, PlayerElement), PlayerElement> elementCombinations =
        new Dictionary<(PlayerElement, PlayerElement), PlayerElement>()
    {
        { (PlayerElement.Fire, PlayerElement.Water), PlayerElement.Steam },
        { (PlayerElement.Water, PlayerElement.Fire), PlayerElement.Steam },
        { (PlayerElement.Fire, PlayerElement.Wind), PlayerElement.Firestorm },
        { (PlayerElement.Wind, PlayerElement.Fire), PlayerElement.Firestorm },
        { (PlayerElement.Water, PlayerElement.Ice), PlayerElement.Mud },
        { (PlayerElement.Ice, PlayerElement.Water), PlayerElement.Mud },
        { (PlayerElement.Wind, PlayerElement.Ice), PlayerElement.Sandstorm },
        { (PlayerElement.Ice, PlayerElement.Wind), PlayerElement.Sandstorm },
    };

   

    public void RegisterButton(ButtonScaler button)
    {
        if (!buttons.Contains(button))
            buttons.Add(button);
    }

    public void OnButtonClicked(ButtonScaler clicked)
    {
        PlayerSO.Instance.currentElement_Q = clicked.stateToSet;
        Debug.Log(clicked.stateToSet);

        if (popupAnimator != null)
            popupAnimator.Hide();

        clicked.SetSelected();
        UpdateUI();
        UpdatePlayerElement();
    }


    private void UpdateUI()
    {
       
    }

    public void UpdatePlayerElement()
    {
        PlayerElement e1 = PlayerSO.Instance.saved1;
        PlayerElement e2 = PlayerSO.Instance.saved2;

        if (elementCombinations.TryGetValue((e1, e2), out PlayerElement combined))
            PlayerSO.Instance.currentElement_E = combined;
    }
}
