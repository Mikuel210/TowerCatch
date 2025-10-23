using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : Singleton<LevelManager>
{

    private static LevelManager _instance;

    [SerializeField] private int startingLevelIndex;
    public static int LevelIndex { get; private set; }
    public static int LevelCount => Instance.levels.Count;
    [SerializeField] private List<LevelSO> levels;

    public event Action GameEnded;
    
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

        if (LevelIndex < levels.Count) {
            int sceneIndex = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(sceneIndex);
            return;
        }
        
        GameEnded?.Invoke();
    }

}
