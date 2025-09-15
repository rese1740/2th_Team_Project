using UnityEngine;
using UnityEngine.UI;

public class PlayerUIController : MonoBehaviour
{
    private GameObject currentNpc = null;

    [Header("Gameobject")]
    public UIPopupAnimator elementPanel;
    public GameObject InventoryPanel;
    public Image hpImg;
    public Image mpImg;

    private void Update()
    {
        hpImg.fillAmount = PlayerSO.Instance.currentHealth / PlayerSO.Instance.maxHealth;
        mpImg.fillAmount = PlayerSO.Instance.rageValue / 100f;

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            InventoryPanel.SetActive(!InventoryPanel.activeSelf);
            UIStateManager.Instance.isUIOpen = InventoryPanel.activeSelf;
        }

        if (UIStateManager.Instance.isUIOpen) return;

        if (Input.GetMouseButtonDown(1))
        {
            OpenNpcWindow(currentNpc);
            UIStateManager.Instance.isUIOpen = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("NPC"))
        {
            currentNpc = other.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("NPC") && other.gameObject == currentNpc)
        {
            currentNpc = null;
        }
    }

    private void OpenNpcWindow(GameObject npc)
    {
        Debug.Log($"NPC {npc.name}와 상호작용 시작");
        // 여기에 UI 활성화 코드
        npc.GetComponent<NPC>().OpenWindow(); // 예시
    }
}
