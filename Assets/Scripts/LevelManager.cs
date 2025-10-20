using System.Collections.Generic;
using UnityEngine;

public class LevelManager : Singleton<LevelManager>
{

    private static LevelManager _instance;
    
    [field: SerializeField] public int LevelIndex { get; private set; }
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

}
