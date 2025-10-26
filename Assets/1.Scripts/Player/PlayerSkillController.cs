using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

public class PlayerSkillController : MonoBehaviour
{
    public PlayerElement currentElement_Q;
    public PlayerElement currentElement_E;

    [Tooltip("플레이어 스킬 목록")]
    Rigidbody2D rb;
    [SerializeField] private List<SkillData> skillList; 

    private Dictionary<(PlayerElement, KeyCode), SkillData> skillMap;
    private Dictionary<(PlayerElement, KeyCode), float> cooldownTimers = new();

    private PlayerMovement pm;
    private PlayerAttack pa;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        pm = GetComponent<PlayerMovement>();
        pa = GetComponent<PlayerAttack>();
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
        if (pa.isRaging)
        {
            return;
        }

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

            cooldownTimers[skillKey] = skill.coolTime;
            switch (skill.skillType)
            {
                case SkillType.Projectile:
                    UseProjectileSkill(skill);
                    break;

                case SkillType.Summon:
                    UseSummonSkill(skill);
                    break;

                case SkillType.Move:
                    if (skill.canDash)
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

        IceProjectile skill_ = projectile.GetComponent<IceProjectile>();

        if(skill != null)
        {
            skill_.damage = skill.damage;
        }

        if (!pm.facingRight)
        {
            Vector3 scale = projectile.transform.localScale;
            scale.x *= -1; 
            projectile.transform.localScale = scale;
        }

        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            float speed = skill.projectileSpeed;
            rb.velocity = new Vector2(pm.facingRight ? speed : -speed, 0f);
        }
    }

    private IEnumerator UseDashSkill(SkillData skill)
    {
        yield return new WaitForFixedUpdate(); 

        skill.canDash = false;
        skill.isDashing = true;

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.velocity = new Vector2(transform.localScale.x * skill.dashingPower, 0f);

        yield return new WaitForSeconds(skill.dashDuration);

        rb.gravityScale = originalGravity;
        skill.isDashing = false;

        yield return new WaitForSeconds(skill.coolTime);
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
