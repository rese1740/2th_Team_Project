using UnityEngine;

public class PlayerElementManager : MonoBehaviour
{
    [Header("속성별 ElementSO 참조")]
    public ElementSO fireSO;
    public ElementSO waterSO;
    public ElementSO iceSO;
    public ElementSO windSO;

    [Header("현재 활성 PlayerSO")]
    public PlayerSO playerSO;

    [Header("사운드 설정")]
    public AudioClip WindSFX;
    public AudioClip FireSFX;
    public AudioClip WaterSFX;
    public AudioClip IceSFX;

    private ElementSO currentElementSO;

    public void ChangeElement(PlayerElement newElement)
    {
        switch (newElement)
        {
            case PlayerElement.Fire:
                currentElementSO = fireSO;
                SoundManager.Instance.PlaySFX(FireSFX);
                break;
            case PlayerElement.Water:
                currentElementSO = waterSO;
                SoundManager.Instance.PlaySFX(WaterSFX);
                break;
            case PlayerElement.Ice:
                currentElementSO = iceSO;
                SoundManager.Instance.PlaySFX(IceSFX);
                break;
            case PlayerElement.Wind:
                currentElementSO = windSO;
                SoundManager.Instance.PlaySFX(WindSFX);
                break;
            default:
                currentElementSO = null;
                break;
        }

        if (currentElementSO != null)
        {
            ApplyStats(currentElementSO);
            playerSO.currentElement_Q = newElement;
            Debug.Log($"{newElement} 속성으로 변경됨 (공격력: {playerSO.attackPower})");
        }
    }

    private void ApplyStats(ElementSO elementSO)
    {
        playerSO.attackPower = elementSO.attackPower;
        playerSO.critValue = elementSO.critChance;
        playerSO.critPower = elementSO.critPower;
        playerSO.rageGainRate = elementSO.rageGainRate;
        playerSO.moveSpeed = elementSO.moveSpeed;

        playerSO.rageAttack = elementSO.rageAttack;
        playerSO.rageHPDecrease = elementSO.rageHPDecrease;
    }
}
