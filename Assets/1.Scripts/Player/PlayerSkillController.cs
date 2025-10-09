using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillController : MonoBehaviour
{
    public PlayerElement currentElement_Q;
    public PlayerElement currentElement_E;

    [Tooltip("플레이어 스킬 목록")]
    [SerializeField] private List<SkillData> skillList; 

    private Dictionary<(PlayerElement, KeyCode), SkillData> skillMap;
    private Dictionary<(PlayerElement, KeyCode), float> cooldownTimers = new();

    private PlayerMovement pm;

    private void Awake()
    {
        pm = GetComponent<PlayerMovement>();
        skillMap = new Dictionary<(PlayerElement, KeyCode), SkillData>();
        currentElement_Q = PlayerElement.None;
        currentElement_E = PlayerElement.None;

        foreach (var skill in skillList)
        {
            skillMap[(skill.elementType, skill.key)] = skill;
        }
    }

    private void Update()
    {
        HandleCooldowns();

        currentElement_Q = PlayerSO.Instance.currentElement_Q;
        currentElement_E = PlayerSO.Instance.currentElement_E;
        if (Input.GetKeyDown(KeyCode.Q)) UseSkill(KeyCode.Q);
        if (Input.GetKeyDown(KeyCode.E)) UseSkill(KeyCode.E);
    }

    private void UseSkill(KeyCode key)
    {
        if(currentElement_Q == PlayerElement.None && currentElement_E == PlayerElement.None)
        {
            return;
        }

        PlayerElement element = key switch
        {
            KeyCode.Q => currentElement_Q,
            KeyCode.E => currentElement_E,
            _ => PlayerElement.None
        };

        var skillKey = (element, key);

        if (skillMap.TryGetValue(skillKey, out SkillData skill))
        {
            if (cooldownTimers.TryGetValue(skillKey, out float timeRemaining) && timeRemaining > 0f)
            {
                Debug.Log($"{skill.skillName}은 {timeRemaining:F1}초 남음");
                return;
            }

            Debug.Log($"사용한 스킬: {skill.skillName}");

            cooldownTimers[skillKey] = skill.cooldown;
            switch (skill.skillType)
            {
                case SkillType.Projectile:
                    UseProjectileSkill(skill);
                    break;

                case SkillType.Summon:
                    UseSummonSkill(skill);
                    break;

                case SkillType.Move:
                    StartCoroutine(UseDashSkill(skill));
                    break;

                case SkillType.Buff:
                    StartCoroutine(UseBuffSkill(skill));
                    break;

                default:
                    Debug.LogWarning("스킬 타입이 지정되지 않음");
                    break;
            }
        }
    }
    private void UseSummonSkill(SkillData skill)
    {
        Debug.Log("발사체 스킬 실행");
        Vector3 spawnPos = transform.position + new Vector3(pm.facingRight ? 1f : -1f, 0f, 0f);

        Instantiate(skill.skillEffectPrefab, spawnPos, Quaternion.identity);
    }

    private void UseProjectileSkill(SkillData skill)
    {
        Debug.Log("발사체 스킬 실행");
        Vector3 spawnPos = transform.position + new Vector3(pm.facingRight ? 1f : -1f, 0f, 0f);

        GameObject projectile = Instantiate(skill.skillEffectPrefab, spawnPos, Quaternion.identity);

        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            float speed = skill.projectileSpeed; 
            rb.velocity = new Vector2(pm.facingRight ? speed : -speed, 0f);
        }
    }

    private IEnumerator UseDashSkill(SkillData skill)
    {
        if (!skill.canDash || skill.isDashing)
            yield break;

        skill.isDashing = true;
        skill.canDash = false;

        Debug.Log("도약 스킬 실행");

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null) yield break;

        float originalGravity = rb.gravityScale;

        rb.gravityScale = 0f;
        rb.velocity = Vector2.zero;

        Vector2 dashDirection = pm.facingRight ? Vector2.right : Vector2.left;

        bool isGrounded = pm.isGrounded; 
        Vector2 finalDirection = isGrounded
            ? dashDirection
            : (dashDirection + Vector2.up * 0.3f).normalized;

        float dashForce = isGrounded ? skill.dashForce_Ground : skill.dashForce_Air;

        if (skill.skillEffectPrefab != null)
            Instantiate(skill.skillEffectPrefab, transform.position, Quaternion.identity);

        rb.AddForce(finalDirection * dashForce, ForceMode2D.Impulse);

        yield return new WaitForSeconds(skill.dashDuration * 0.7f);
        rb.gravityScale = originalGravity;

        yield return new WaitForSeconds(skill.dashDuration * 0.3f);
        rb.velocity = Vector2.zero;

        skill.isDashing = false;

        yield return new WaitForSeconds(skill.cooldown);
        skill.canDash = true;
    }
    IEnumerator UseBuffSkill(SkillData skill)
    {
        Debug.Log("버프 스킬 실행");
        yield return null;
    }



    private void HandleCooldowns()
    {
        var keys = new List<(PlayerElement, KeyCode)>(cooldownTimers.Keys);

        foreach (var key in keys)
        {
            if (cooldownTimers[key] > 0f)
            {
                cooldownTimers[key] -= Time.deltaTime;

                if (cooldownTimers[key] < 0f)
                    cooldownTimers[key] = 0f;
            }
        }
    }
}
