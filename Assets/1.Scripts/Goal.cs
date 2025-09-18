using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerGoal : MonoBehaviour
{
    [Header("Target Scene")]
    [Tooltip("플레이어가 Goal에 닿았을 때 이동할 씬 이름")]
    public string sceneName = "Test_Scene 1";

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Goal"))
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}