using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempSoundPlayer : MonoBehaviour
{
    public AudioClip bgmClip;       // 배경음악 오디오 클립
    public AudioClip sfxClip;       // 효과음 오디오 클립

    public float bgmVolume = 1f;    // 배경음악 크기
    public float sfxVolume = 1f;    // 효과음 크기
        

     // 배경음악 재생 예시
    void PlayBGM()
    {
        // 음악 재생
        SoundManager.Instance.PlayBGM(bgmClip);

        // 음악 정지
        SoundManager.Instance.StopBGM();

        // 음악 볼륨 설정
        SoundManager.Instance.SetBGMVolume(bgmVolume);
    }

    // 효과음 재생
    void PlaySFX()
    {
        // 효과음 재생 (기본 볼륨 = 1)
        SoundManager.Instance.PlaySFX(sfxClip);

        // 효과음 재생 (볼륨 설정 -> 전체 볼륨 기준 배율)
        SoundManager.Instance.PlaySFX(sfxClip, sfxVolume);

        // 효과음 크기 조절 (전체 볼륨 조절)
        SoundManager.Instance.SetSFXVolume(sfxVolume);
    }
}
