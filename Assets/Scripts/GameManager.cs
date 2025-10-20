using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    private bool won;

    private void Start() => CatchPin.Instance.OnCatch += () => won = true;

    private void Update()
    {
        if (won && Input.GetKeyDown(KeyCode.Space))
            LevelManager.Instance.NextLevel();
        
        if (!Input.GetKeyDown(KeyCode.R)) return;
        SceneManager.LoadScene(0);
    }
    
}
