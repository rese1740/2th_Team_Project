using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerGoal : MonoBehaviour
{
    [Header("Target Scene")]
    [Tooltip("�÷��̾ Goal�� ����� �� �̵��� �� �̸�")]
    public string sceneName = "Test_Scene 1";

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Goal"))
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}