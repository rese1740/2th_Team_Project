using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIHPBar : MonoBehaviour
{
    [Header("HP Data (HpSO)")]
    [Tooltip("HpSO 불러오기")]
    public HpSO hpData;

    [Header("HpUI")]
    [Tooltip("HpSlider 불러오기")]
    public Slider hpSlider;

    [Header("HpText")]
    [Tooltip("Hp숫자 표기")]
    public bool updateText = false;
    public TMP_Text hpText;

    private void Start()
    {
        if (hpData != null && hpSlider != null)
        {
            hpSlider.maxValue = hpData.maxHealth;
            hpSlider.value = hpData.currentHealth;
        }
    }

    private void Update()
    {
        if (hpData == null || hpSlider == null) return;

        hpSlider.maxValue = hpData.maxHealth;
        hpSlider.value = hpData.currentHealth;

        if (updateText && hpText != null)
        {
            hpText.text = $"{hpData.currentHealth:0}/{hpData.maxHealth:0}";
        }
    }
}
