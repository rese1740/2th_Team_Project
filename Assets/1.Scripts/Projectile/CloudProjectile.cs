using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudProjectile : MonoBehaviour
{
    public float moveSpeed = 1f;          // 초당 1타일 = 1 거리
    public float rainInterval = 0.25f;    // 초당 4개 → 1/4초마다 1개
    public float cloudDuration = 5f;      // 먹구름 유지 시간
    public GameObject rainPrefab;         // 비 발사체

    private float rainTimer = 0f;

    private void Start()
    {
        Destroy(gameObject, cloudDuration);
    }

    private void Update()
    {
        rainTimer += Time.deltaTime;
        if (rainTimer >= rainInterval)
        {
            rainTimer = 0f;
            SpawnRain();
        }
    }

    void SpawnRain()
    {
        Instantiate(rainPrefab, transform.position, Quaternion.identity);
    }
}
