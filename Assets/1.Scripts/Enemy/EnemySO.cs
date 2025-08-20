using UnityEngine;

[CreateAssetMenu(fileName = "EnemySO", menuName = "EnemySO")]
public class EnemySO : ScriptableObject
{
    [Header("Patrol")]
    public float patrolSpeed = 3f;

    [Header("Detection")]
    public float detectRangeTiles = 5f;
    public float tileSize = 1f;

    [Header("Alert")]
    public GameObject alertIconPrefab;
    public Vector3 alertOffset = new Vector3(0, 1.5f, 0);
    public float alertDuration = 0.3f;
    public float bounceHeight = 0.3f;
    public float bounceSpeed = 6f;

    [Header("Ranged Attack")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 8f;
    public float attackInterval = 0.8f;
    public bool attackOnlyHorizontal = false;

    [Header("Aggro/Chase")]
    public bool persistAggro = true;     // �ѹ� �ɸ��� ��׷� ����
    public bool chaseWhileAggro = true;  // ��׷� ���¿��� �߰�����
    public float chaseSpeed = 4.5f;      // �߰� �ӵ�
}
