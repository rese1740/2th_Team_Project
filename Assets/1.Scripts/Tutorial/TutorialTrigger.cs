using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    [Header("튜토리얼 텍스트")]
    [TextArea] public string tutorialMessage;

    [Header("완료 키")]
    public KeyCode requiredKey = KeyCode.Space;

    [Header("마우스 클릭")]
    public bool allowMouseClick = true;

    public int groupIndex;

    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;

        TutorialManager.Instance.ShowTutorial(
            tutorialMessage,
            groupIndex,
            requiredKey,
            allowMouseClick
            );

        Debug.Log("ShowTutorial 실행됨");
    }
}
