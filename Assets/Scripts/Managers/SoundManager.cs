using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance = null;
    public AudioSource bgmSource;
    public AudioClip[] bgmClips;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StopBGM(scene, mode);
        PlayBGM();
    }

    public void PlayBGM()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName == "TitleScene")
        {
            bgmSource.clip = bgmClips[0];
        }
        else if (sceneName == "SinglePlayScene")
        {
            bgmSource.clip = bgmClips[1];
        }
        else if (sceneName == "MultiPlayScene")
        {
            bgmSource.clip = bgmClips[2];
        }

        if (bgmSource.clip != null)
        {
            bgmSource.Play();
            bgmSource.loop = true;
        }
    }

    public void StopBGM(Scene scene, LoadSceneMode mode)
    {
        if (bgmSource != null && bgmSource.isPlaying)
        {
            bgmSource.Stop();
        }
    }

    public void SetVolume(float volume)
    {
        bgmSource.volume = volume;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
