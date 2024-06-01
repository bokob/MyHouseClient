using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    // 0 == MainMenu bgm / 1 == Game Scene bgm
    public static SoundManager Soundinstance = null;
    public AudioSource bgmSource;
    public AudioSource sfxSource;
    public AudioClip[] bgmClips;
    public AudioClip[] sfxClips;

    private void Awake() {
        if(Soundinstance == null)
        {
            Soundinstance = this;
        }
        else if (Soundinstance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }
    public void PlayBGM(int index)
    {
        if (index < bgmClips.Length)
        {
            bgmSource.clip = bgmClips[index];
            bgmSource.Play();
            bgmSource.loop = true;
        }
    }
    public void StopBGM()
    {
        bgmSource.Stop();
    }

    public void PlaySFX(int index)
    {
        if (index < sfxClips.Length)
        {
            sfxSource.PlayOneShot(sfxClips[index]);
        }
    }

    public void SetVolume(float volume)
    {
        bgmSource.volume = volume;
        sfxSource.volume = volume;
    }
}
