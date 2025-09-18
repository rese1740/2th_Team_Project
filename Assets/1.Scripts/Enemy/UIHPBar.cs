using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIHPBar : MonoBehaviour
{
    [Header("HP Data (HpSO)")]
    [Tooltip("HpSO �ҷ�����")]
    public HpSO hpData;

    [Header("HpUI")]
    [Tooltip("HpSlider �ҷ�����")]
    public Slider hpSlider;

    [Header("HpText")]
    [Tooltip("Hp���� ǥ��")]
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
