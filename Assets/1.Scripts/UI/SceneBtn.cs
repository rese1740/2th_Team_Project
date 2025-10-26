using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneBtn : MonoBehaviour
{
    public void Restart()
    {
        SceneManager.LoadScene("Test_Scene");   
    }
}
