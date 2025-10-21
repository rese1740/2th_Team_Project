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

    private ElementSO currentElementSO;

    public void ChangeElement(PlayerElement newElement)
    {
        switch (newElement)
        {
            case PlayerElement.Fire:
                currentElementSO = fireSO;
                break;
            case PlayerElement.Water:
                currentElementSO = waterSO;
                break;
            case PlayerElement.Ice:
                currentElementSO = iceSO;
                break;
            case PlayerElement.Wind:
                currentElementSO = windSO;
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
