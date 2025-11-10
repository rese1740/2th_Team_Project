using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public InventorySlot[] inventorySlots;

    public Image[] elementSlots;

    public Sprite[] elementIcon;

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
                break;

            case PlayerElement.Water:
                elementSlots[0].sprite = elementIcon[1];
                break;

            case PlayerElement.Wind:
                elementSlots[0].sprite = elementIcon[2];
                break;

            case PlayerElement.Ice:
                elementSlots[0].sprite = elementIcon[3];
                break;
        }

        switch (PlayerSO.Instance.saved2)
        {
            case PlayerElement.Fire:
                elementSlots[1].sprite = elementIcon[0];
                break;

            case PlayerElement.Water:
                elementSlots[1].sprite = elementIcon[1];
                break;

            case PlayerElement.Wind:
                elementSlots[1].sprite = elementIcon[2];
                break;

            case PlayerElement.Ice:
                elementSlots[1].sprite = elementIcon[3];
                break;
        }
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
