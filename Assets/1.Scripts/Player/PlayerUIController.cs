using UnityEngine;

public class PlayerUIController : MonoBehaviour
{

    private GameObject currentNpc = null;

    [Header("Gameobject")]
    public UIPopupAnimator elementPanel;
    public GameObject InventoryPanel;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab))
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
        if (Input.GetKeyDown(KeyCode.V))
        {
            elementPanel.Show();
            UIStateManager.Instance.isUIOpen = true;
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
