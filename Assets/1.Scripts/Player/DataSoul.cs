using UnityEngine;
using static UnityEditor.Rendering.FilterWindow;

public class DataSoul : MonoBehaviour
{
    public string nextSceneName;
    private bool isPlayerInRange = false;
    public PlayerElement element;

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.Space))
        {
            ActivateSoul();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }

    void ActivateSoul()
    {
       
        if(PlayerSO.Instance.dataPiece <= 29)
            return;

        PlayerSO.Instance.unlockedElements.Add(element);
        PlayerSO.Instance.dataPiece -= 30;
        SceneFadeManager.Instance.SceneMove(nextSceneName);

    }
}
