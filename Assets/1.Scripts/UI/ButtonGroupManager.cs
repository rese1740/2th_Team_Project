using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonGroupManager : MonoBehaviour
{
    public static ButtonGroupManager Instance;

    private List<ButtonScaler> buttons = new List<ButtonScaler>();
    public List<ButtonScaler> selectedButtons = new List<ButtonScaler>();

    [Header("선택된 버튼을 표시할 UI 슬롯")]
    public Image slot1Icon;
    public Image slot2Icon;
    public UIPopupAnimator popupAnimator;
    public PlayerElementManager playerElementManager;
    public GameObject[] ElementEffecct;
    private GameObject player;
    public bool isChangedElement = false;

    public int selectedCount = 0;

    private Dictionary<(PlayerElement, PlayerElement), PlayerElement> elementCombinations =
        new Dictionary<(PlayerElement, PlayerElement), PlayerElement>()
    {
        { (PlayerElement.Fire, PlayerElement.Water), PlayerElement.Steam },
        { (PlayerElement.Water, PlayerElement.Fire), PlayerElement.Steam },
        { (PlayerElement.Fire, PlayerElement.Wind), PlayerElement.Firestorm },
        { (PlayerElement.Wind, PlayerElement.Fire), PlayerElement.Firestorm },
        { (PlayerElement.Water, PlayerElement.Ice), PlayerElement.IceWater },
        { (PlayerElement.Ice, PlayerElement.Water), PlayerElement.IceWater },
        { (PlayerElement.Wind, PlayerElement.Ice), PlayerElement.Sandstorm },
        { (PlayerElement.Ice, PlayerElement.Wind), PlayerElement.Sandstorm },
    };

    private void Start()
    {
        player = GameObject.FindWithTag("Player");
        Instance = this;
    }

    public void RegisterButton(ButtonScaler button)
    {
        if (!buttons.Contains(button))
            buttons.Add(button);
    }

    public void OnButtonClicked(ButtonScaler clicked)
    {
        PlayerSO.Instance.currentElement_Q = clicked.stateToSet;
        playerElementManager.ChangeElement(clicked.stateToSet);

        if (popupAnimator != null)
            popupAnimator.Hide();

        clicked.SetSelected();
        UpdatePlayerElement();

       isChangedElement = true;
        Vector2 spawnPos = player.transform.position + new Vector3(0, 2.5f);
        GameObject effectInstance = null;

        switch (clicked.stateToSet)
        {
            case PlayerElement.Fire:
                effectInstance = Instantiate(ElementEffecct[0], spawnPos, Quaternion.identity);
                break;
            case PlayerElement.Water:
                effectInstance = Instantiate(ElementEffecct[1], spawnPos, Quaternion.identity);
                break;
            case PlayerElement.Ice:
                effectInstance = Instantiate(ElementEffecct[2], spawnPos, Quaternion.identity);
                break;
            case PlayerElement.Wind:
                effectInstance = Instantiate(ElementEffecct[3], spawnPos, Quaternion.identity);
                break;
        }

        effectInstance.transform.SetParent(player.transform);
    }

    public void UpdatePlayerElement()
    {
        PlayerElement e1 = PlayerSO.Instance.saved1;
        PlayerElement e2 = PlayerSO.Instance.saved2;

        if (elementCombinations.TryGetValue((e1, e2), out PlayerElement combined))
            PlayerSO.Instance.currentElement_E = combined;
    }
}
