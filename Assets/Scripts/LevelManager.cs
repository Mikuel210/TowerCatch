using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : Singleton<LevelManager>
{

    private static LevelManager _instance;

    [SerializeField] private int startingLevelIndex;
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

        LevelIndex = startingLevelIndex;
    }

    public LevelSO GetLevel() => levels[LevelIndex];
    public void NextLevel()
    {
        LevelIndex++;

        int sceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(sceneIndex);
    }

}
