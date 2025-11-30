using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public InventorySlot[] inventorySlots;

    public Image[] elementSlots;

    public Sprite[] elementIcon;

    public Image[] skillSlots;

    public Sprite[] skillIcons;

    public Text[] playerStatusTxt;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        switch (PlayerSO.Instance.saved1)
        {
            case PlayerElement.Fire:
                elementSlots[0].sprite = elementIcon[0];
                skillSlots[0].sprite = skillIcons[0];
                break;

            case PlayerElement.Water:
                elementSlots[0].sprite = elementIcon[1];
                skillSlots[0].sprite = skillIcons[1];
                break;

            case PlayerElement.Wind:
                elementSlots[0].sprite = elementIcon[2];
                skillSlots[0].sprite = skillIcons[2];
                break;

            case PlayerElement.Ice:
                elementSlots[0].sprite = elementIcon[3];
                skillSlots[0].sprite = skillIcons[3];
                break;
        }

        switch (PlayerSO.Instance.saved2)
        {
            case PlayerElement.Fire:
                elementSlots[1].sprite = elementIcon[0];
                skillSlots[1].sprite = skillIcons[0];
                break;

            case PlayerElement.Water:
                elementSlots[1].sprite = elementIcon[1];
                skillSlots[1].sprite = skillIcons[1];
                break;

            case PlayerElement.Wind:
                elementSlots[1].sprite = elementIcon[2];
                skillSlots[1].sprite = skillIcons[2];
                break;

            case PlayerElement.Ice:
                elementSlots[1].sprite = elementIcon[3];
                skillSlots[1].sprite = skillIcons[3];
                break;
        }

        switch (PlayerSO.Instance.currentElement_E)
        {
            case PlayerElement.IceWind:
                skillSlots[2].sprite = skillIcons[5];
                break;

            case PlayerElement.IceWater:
                skillSlots[2].sprite = skillIcons[6];
                break;

            case PlayerElement.IceFire:
                skillSlots[2].sprite = skillIcons[7];
                break;

            case PlayerElement.WaterFire:
                skillSlots[2].sprite = skillIcons[8];
                break;

            case PlayerElement.WaterWind:
                skillSlots[2].sprite = skillIcons[9];
                break;

            case PlayerElement.FireWind:
                skillSlots[2].sprite = skillIcons[4];

                break;
        }

        playerStatusTxt[0].text = $"HP: {PlayerSO.Instance.currentHealth} / {PlayerSO.Instance.maxHealth}";
        playerStatusTxt[1].text = $"공격력: {PlayerSO.Instance.attackPower}";
        playerStatusTxt[2].text = $"크리티컬 확률: {PlayerSO.Instance.critValue}%";
        playerStatusTxt[3].text = $"크리티컬 데미지: {PlayerSO.Instance.critPower}%";
        playerStatusTxt[4].text =  $"폭주 게이지: {PlayerSO.Instance.rageValue.ToString("F0")}%";


    }

    public bool AddItem(ItemData item)
    {
        foreach (InventorySlot slot in inventorySlots)
        {   
            if (slot.IsEmpty)
            {
                slot.SetItem(item);
                return true;
            }
        }

        Debug.Log("인벤토리 공간이 부족합니다!");
        return false;
    }
}
