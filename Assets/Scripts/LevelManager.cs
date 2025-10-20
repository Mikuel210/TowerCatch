using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : Singleton<LevelManager>
{

    private static LevelManager _instance;

    private static int LevelIndex { get; set; }
    [SerializeField] private List<LevelSO> levels;
    
    void Awake()
    {
        if (_instance != null) {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public LevelSO GetLevel() => levels[LevelIndex];
    public void NextLevel()
    {
        LevelIndex++;

        int sceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(sceneIndex);
    }

}
