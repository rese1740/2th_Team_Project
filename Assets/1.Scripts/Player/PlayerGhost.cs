using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGhost : MonoBehaviour
{
    public GameObject ghostPrefab;
    public float spawnRate = 0.05f;   
    public float ghostDuration = 0.3f; 

    float timer = 0f;
    bool isGhosting = false;
    SpriteRenderer playerSR;

    void Start()
    {
        playerSR = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (!isGhosting) return;

        timer += Time.deltaTime;
        if (timer >= spawnRate)
        {
            CreateGhost();
            timer = 0;
        }
    }

    public void StartGhost()
    {
        isGhosting = true;
    }

    public void StopGhost()
    {
        isGhosting = false;
    }

    void CreateGhost()
    {
        GameObject g = Instantiate(ghostPrefab, transform.position, transform.rotation);
        SpriteRenderer sr = g.GetComponent<SpriteRenderer>();

        sr.sprite = playerSR.sprite;          // 현재 스프라이트 복사
        sr.flipX = playerSR.flipX;            // 방향 맞추기
        g.transform.localScale = transform.localScale;

        Destroy(g, ghostDuration);            // 잔상 자동삭제
    }
}
