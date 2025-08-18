using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

public enum EAudioMixerType { Master, BGM, SFX }

public class AudioManager : MonoBehaviour
{
    [Header("Mixer")]
    [SerializeField] private AudioMixer audioMixer; // Master/BGM/SFX �Ķ���� ����Ǿ� �־�� ��

    [Header("Sliders (0~1)")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Optional: Percent Labels")]
    [SerializeField] private TMP_Text masterValueText;
    [SerializeField] private TMP_Text bgmValueText;
    [SerializeField] private TMP_Text sfxValueText;

    [Header("Optional: Mute Toggles")]
    [SerializeField] private Toggle masterMuteToggle;
    [SerializeField] private Toggle bgmMuteToggle;
    [SerializeField] private Toggle sfxMuteToggle;

    // ���� ����(0~1)
    private float[] savedVolumes = new float[3] { 1f, 1f, 1f };
    private bool[] isMute = new bool[3];

    // PlayerPrefs Ű (���ϸ� ���ų� Ű ���� ����)
    private const string PP_MASTER = "vol_master";
    private const string PP_BGM = "vol_bgm";
    private const string PP_SFX = "vol_sfx";
    private const string PP_M_MUTE = "mute_master";
    private const string PP_B_MUTE = "mute_bgm";
    private const string PP_S_MUTE = "mute_sfx";

    void Awake()
    {
        // ����� �� �ҷ����� (������ 1)
        savedVolumes[(int)EAudioMixerType.Master] = PlayerPrefs.GetFloat(PP_MASTER, 1f);
        savedVolumes[(int)EAudioMixerType.BGM] = PlayerPrefs.GetFloat(PP_BGM, 1f);
        savedVolumes[(int)EAudioMixerType.SFX] = PlayerPrefs.GetFloat(PP_SFX, 1f);

        isMute[(int)EAudioMixerType.Master] = PlayerPrefs.GetInt(PP_M_MUTE, 0) == 1;
        isMute[(int)EAudioMixerType.BGM] = PlayerPrefs.GetInt(PP_B_MUTE, 0) == 1;
        isMute[(int)EAudioMixerType.SFX] = PlayerPrefs.GetInt(PP_S_MUTE, 0) == 1;

        // �ͼ��� �ݿ�
        ApplyVolumeToMixer(EAudioMixerType.Master, GetEffectiveVolume01(EAudioMixerType.Master));
        ApplyVolumeToMixer(EAudioMixerType.BGM, GetEffectiveVolume01(EAudioMixerType.BGM));
        ApplyVolumeToMixer(EAudioMixerType.SFX, GetEffectiveVolume01(EAudioMixerType.SFX));
    }

    void Start()
    {
        // UI �ʱ�ȭ(�����̴�/��� �� ���簪 �ݿ�, �̺�Ʈ ����)
        InitSlider(masterSlider, EAudioMixerType.Master);
        InitSlider(bgmSlider, EAudioMixerType.BGM);
        InitSlider(sfxSlider, EAudioMixerType.SFX);

        SetValueText(masterValueText, masterSlider ? masterSlider.value : savedVolumes[(int)EAudioMixerType.Master]);
        SetValueText(bgmValueText, bgmSlider ? bgmSlider.value : savedVolumes[(int)EAudioMixerType.BGM]);
        SetValueText(sfxValueText, sfxSlider ? sfxSlider.value : savedVolumes[(int)EAudioMixerType.SFX]);

        InitMuteToggle(masterMuteToggle, EAudioMixerType.Master);
        InitMuteToggle(bgmMuteToggle, EAudioMixerType.BGM);
        InitMuteToggle(sfxMuteToggle, EAudioMixerType.SFX);
    }

    // ---------- UI �ʱ�ȭ & �̺�Ʈ ----------
    private void InitSlider(Slider slider, EAudioMixerType type)
    {
        if (!slider) return;
        slider.minValue = 0f; slider.maxValue = 1f;

        // ���尪(0~1)�� ǥ�� (��Ʈ ���¿��� �����̴��� "���� ����" ����)
        slider.SetValueWithoutNotify(savedVolumes[(int)type]);

        slider.onValueChanged.AddListener(v =>
        {
            savedVolumes[(int)type] = Mathf.Clamp01(v);
            ApplyVolumeToMixer(type, GetEffectiveVolume01(type)); // ��Ʈ�� 0.0001, �ƴϸ� v
            // �� ����
            if (type == EAudioMixerType.Master) SetValueText(masterValueText, v);
            else if (type == EAudioMixerType.BGM) SetValueText(bgmValueText, v);
            else SetValueText(sfxValueText, v);
            SavePrefs();
        });
    }

    private void InitMuteToggle(Toggle toggle, EAudioMixerType type)
    {
        if (!toggle) return;
        toggle.SetIsOnWithoutNotify(isMute[(int)type]);
        toggle.onValueChanged.AddListener(isOn =>
        {
            isMute[(int)type] = isOn;
            ApplyVolumeToMixer(type, GetEffectiveVolume01(type));
            SavePrefs();
        });
    }

    // ---------- ����/��Ʈ ���� ----------
    private float GetEffectiveVolume01(EAudioMixerType type)
    {
        // ��Ʈ�� ���� 0 (�α� ��ȯ ��ȣ)
        if (isMute[(int)type]) return 0.0001f;
        return Mathf.Clamp01(savedVolumes[(int)type]);
    }

    private void ApplyVolumeToMixer(EAudioMixerType type, float volume01)
    {
        // 0~1 �� dB (-80~0)
        float db = Mathf.Log10(Mathf.Clamp(volume01, 0.0001f, 1f)) * 20f;
        audioMixer.SetFloat(type.ToString(), db);
    }

    private void SetValueText(TMP_Text label, float v01)
    {
        if (!label) return;
        label.text = Mathf.RoundToInt(v01 * 100f) + "%";
    }

    // ---------- �ܺο����� �� �� �ִ� API ----------
    public void SetVolume01(EAudioMixerType type, float v01)
    {
        savedVolumes[(int)type] = Mathf.Clamp01(v01);
        ApplyVolumeToMixer(type, GetEffectiveVolume01(type));
        // UI ����ȭ
        var slider = GetSlider(type);
        if (slider) slider.SetValueWithoutNotify(savedVolumes[(int)type]);
        SavePrefs();
    }

    public float GetVolume01(EAudioMixerType type) => savedVolumes[(int)type];

    public void ToggleMute(EAudioMixerType type)
    {
        isMute[(int)type] = !isMute[(int)type];
        ApplyVolumeToMixer(type, GetEffectiveVolume01(type));
        // UI ����ȭ
        var toggle = GetToggle(type);
        if (toggle) toggle.SetIsOnWithoutNotify(isMute[(int)type]);
        SavePrefs();
    }

    // ---------- ���� ----------
    private Slider GetSlider(EAudioMixerType type)
    {
        if (type == EAudioMixerType.Master) return masterSlider;
        else if (type == EAudioMixerType.BGM) return bgmSlider;
        else return sfxSlider;
    }
    private Toggle GetToggle(EAudioMixerType type)
    {
        if (type == EAudioMixerType.Master) return masterMuteToggle;
        else if (type == EAudioMixerType.BGM) return bgmMuteToggle;
        else return sfxMuteToggle;
    }

    private void SavePrefs()
    {
        PlayerPrefs.SetFloat(PP_MASTER, savedVolumes[(int)EAudioMixerType.Master]);
        PlayerPrefs.SetFloat(PP_BGM, savedVolumes[(int)EAudioMixerType.BGM]);
        PlayerPrefs.SetFloat(PP_SFX, savedVolumes[(int)EAudioMixerType.SFX]);
        PlayerPrefs.SetInt(PP_M_MUTE, isMute[(int)EAudioMixerType.Master] ? 1 : 0);
        PlayerPrefs.SetInt(PP_B_MUTE, isMute[(int)EAudioMixerType.BGM] ? 1 : 0);
        PlayerPrefs.SetInt(PP_S_MUTE, isMute[(int)EAudioMixerType.SFX] ? 1 : 0);
        PlayerPrefs.Save();
    }
}