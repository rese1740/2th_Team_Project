using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("BGM")]
    public AudioSource bgmSource;

    [Header("SFX")]
    public AudioSource sfxPrefab;     
    public int sfxPoolSize = 10;       
    private List<AudioSource> sfxPool = new List<AudioSource>();
    private int poolIndex = 0;

    [Header("Volume")]
    [Range(0f, 1f)] public float bgmVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
        }

        CreateSFXPool();
    }

    private void CreateSFXPool()
    {
        for (int i = 0; i < sfxPoolSize; i++)
        {
            AudioSource sfx = Instantiate(sfxPrefab, transform);
            sfx.playOnAwake = false;
            sfxPool.Add(sfx);
        }
    }

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;

        bgmSource.clip = clip;
        bgmSource.volume = bgmVolume;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;

        AudioSource a = sfxPool[poolIndex];
        poolIndex = (poolIndex + 1) % sfxPoolSize;

        a.volume = sfxVolume * volume;
        a.clip = clip;
        a.Play();
    }

    public void SetBGMVolume(float v)
    {
        bgmVolume = v;
        bgmSource.volume = bgmVolume;
    }

    public void SetSFXVolume(float v)
    {
        sfxVolume = v;
    }
}
