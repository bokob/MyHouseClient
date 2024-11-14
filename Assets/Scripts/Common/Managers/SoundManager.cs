using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    public static SoundManager _instance = null;
    public AudioSource _bgmSource;
    public AudioClip[] bgmClips;
    [SerializeField] AudioSource _effectSource;
    [SerializeField] AudioClip[] _effectClips;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        UpdateMusicVolume();
    }

    public void UpdateMusicVolume()
    {
        _bgmSource.volume = PlayerPrefs.GetFloat("MusicVolume");
        _effectSource.volume = PlayerPrefs.GetFloat("MusicVolume");
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StopBGM(scene, mode);
        PlayBGM();
    }

    // BGM 재생
    public void PlayBGM()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName == "TitleScene")
        {
            _bgmSource.clip = bgmClips[0];
        }
        else if (sceneName == "SinglePlayScene")
        {
            _bgmSource.clip = bgmClips[1];
        }
        else if (sceneName == "MultiPlayScene")
        {
            _bgmSource.clip = bgmClips[2];
        }

        if (_bgmSource.clip != null)
        {
            _bgmSource.Play();
            _bgmSource.loop = true;
        }
    }

    public void StopBGM(Scene scene, LoadSceneMode mode)
    {
        if (_bgmSource != null && _bgmSource.isPlaying)
        {
            _bgmSource.Stop();
        }
    }

    // 효과음 재생
    public void PlayEffect(string name)
    {
        _effectSource.clip = null;
        foreach (AudioClip clip in _effectClips)
        {
            if (clip.name == name)
            {
                _effectSource.clip = clip;
            }
        }

        if (_effectSource.clip == null)
        {
            return;
        }
        _effectSource.PlayOneShot(_effectSource.clip);
    }

    // 해당 지점에서 소리 재생
    public void PlayEffectAtPoint(string name, Transform targetTransform)
    {
        _effectSource.clip = null;
        foreach (AudioClip clip in _effectClips)
        {
            if (clip.name == name)
            {
                _effectSource.clip = clip;
            }
        }

        if (_effectSource.clip == null)
        {
            return;
        }
        _effectSource.PlayOneShot(_effectSource.clip);

        AudioSource.PlayClipAtPoint(_effectSource.clip, targetTransform.position);
    }


    public void SetVolume(float volume)
    {
        _bgmSource.volume = volume;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
