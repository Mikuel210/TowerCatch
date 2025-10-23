using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    private bool won;
    private bool gameEnded;

    private void Start()
    {
        CatchPin.Instance.OnCatch += () => won = true;
        LevelManager.Instance.GameEnded += () => gameEnded = true;
    } 

    private void Update()
    {
        if (won && Input.GetKeyDown(KeyCode.Space))
            LevelManager.Instance.NextLevel();
        
        if (!Input.GetKeyDown(KeyCode.R) || gameEnded) return;
        SceneManager.LoadScene(0);
    }
    
}
