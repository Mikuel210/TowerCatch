using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MusicPlayer : MonoBehaviour
{
    
    private static MusicPlayer _instance;
    private AudioSource _audioSource;
    
    [SerializeField] private List<AudioClip> songs;
    private int _songIndex;
    
    void Awake()
    {
        if (_instance != null) {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);

        _audioSource = GetComponent<AudioSource>();
        _audioSource.clip = songs[_songIndex];
        _audioSource.Play();
    }

    private void Update()
    {
        if (_audioSource.isPlaying) return;

        _songIndex = _songIndex == songs.Count - 1 ? 0 : _songIndex + 1;
        _audioSource.clip = songs[_songIndex];
        _audioSource.Play();
    }

}
