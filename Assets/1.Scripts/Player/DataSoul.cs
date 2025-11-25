using UnityEngine;

public class DataSoul : MonoBehaviour
{
    public string nextSceneName;
    private bool isPlayerInRange = false;

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

        PlayerSO.Instance.dataPiece -= 30;
        SceneFadeManager.Instance.SceneMove(nextSceneName);

    }
}
