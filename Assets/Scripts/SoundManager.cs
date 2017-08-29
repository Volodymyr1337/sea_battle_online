using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private static SoundManager _instance;
    public static SoundManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<SoundManager>();
            return _instance;
        }
    }

    [SerializeField] private AudioSource AudioSource;
    [SerializeField] private List<AudioClip> AudioClips;
    [SerializeField] private AudioSource mainSongSource;


    private void Start()
    {
        if (PlayerPrefs.GetInt("Music") == 0)
            mainSongSource.Stop();
    }

    /// <summary>
    /// Нажатие на кнопку
    /// </summary>
    public void ClickButton()
    {
        Play("Click", 0.7f);
    }
    /// <summary>
    /// Ищет звук по имени в списке саундтреков
    /// </summary>
    /// <param name="sound">имя проигрываемого аудиоклипа</param>
    public void Play(string sound, float volume = 1f)
    {
        if (AudioClips != null)
            AudioSource.PlayOneShot(AudioClips.Find(x => x.name == sound), volume);
        else
            Debug.LogError("Массив аудиоклипов пуст!");
    }

    public void MainSongMute()
    {
        if (mainSongSource.isPlaying)
            mainSongSource.Stop();
        else
            mainSongSource.Play();            
    }
}
