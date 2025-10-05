using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.R)) return;
        SceneManager.LoadScene(0);
    }
}
