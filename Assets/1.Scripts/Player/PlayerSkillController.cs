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

    Rigidbody2D rb;
    private PlayerMovement pm;
    private PlayerAttack pa;
    private Animator animator;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        pm = GetComponent<PlayerMovement>();
        pa = GetComponent<PlayerAttack>();
        animator = GetComponent<Animator>();
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
            return;

        if (currentElement_Q == PlayerElement.None && currentElement_E == PlayerElement.None)
            return;

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

            if (!string.IsNullOrEmpty(skill.animationName))
            {
                animator.SetTrigger(skill.animationName);
            }

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
                    UseDashSkill(skill);
                    break;

                case SkillType.Buff:
                    StartCoroutine(UseBuffSkill(skill));
                    break;

                case SkillType.Targeting:
                    UseTargetingSkill(skill);
                    break;

                case SkillType.Enchant:
                    UseEnchantSkill(skill);
                    break;

                default:
                    Debug.LogWarning("스킬 타입이 지정되지 않음");
                    break;
            }
        }
    }
    private void UseSummonSkill(SkillData skill)
    {
        Debug.Log("소환 스킬 실행");
        Vector3 spawnPos = transform.position + new Vector3(pm.facingRight ? 1f : -1f, 0f, 0f);
        GameObject summonPrefab = Instantiate(skill.skillEffectPrefab, spawnPos, Quaternion.identity);

        IceProjectile skill_ = summonPrefab.GetComponent<IceProjectile>();
        PlayerHitBox hitBox = summonPrefab.GetComponent<PlayerHitBox>();



        if (skill_ != null || hitBox != null)
        {
            skill_.damage = skill.damage;
        }

        if (!pm.facingRight)
        {
            Vector3 scale = summonPrefab.transform.localScale;
            scale.x *= -1;
            summonPrefab.transform.localScale = scale;
        }
    }

    private void UseProjectileSkill(SkillData skill)
    {
        Debug.Log("발사체 스킬 실행");

        Vector3 spawnPos = transform.position + new Vector3(pm.facingRight ? 1f : -1f, 0f, 0f);

        GameObject projectile = Instantiate(skill.skillEffectPrefab, spawnPos, Quaternion.identity);

        PlayerHitBox hitBox = projectile.GetComponent<PlayerHitBox>();

        if (hitBox != null)
        {
            hitBox.damage = skill.damage;
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

    private void UseDashSkill(SkillData skill)
    {
        if(pm.canDash && !pm.isDashing)
            pm.StartDash(skill.dashingPower, skill.dashDuration);
    }

    private void UseTargetingSkill(SkillData skill)
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, skill.range, skill.targetLayer);

        if (enemies.Length == 0)
        {
            Debug.Log("타겟 없음!");
            return;
        }

        Transform target = enemies[0].transform;
        float closestDist = Vector2.Distance(transform.position, target.position);

        foreach (var e in enemies)
        {
            float dist = Vector2.Distance(transform.position, e.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                target = e.transform;
            }
        }

        for (int i = 0; i < skill.projectileCount; i++)
        {
            float t = (skill.projectileCount == 1) ? 0.5f : (float)i / (skill.projectileCount - 1);
            float angle = Mathf.Lerp(-90f, 90f, t); // -90° ~ 90° 위쪽 반원

            Vector2 dir = new Vector2(Mathf.Sin(angle * Mathf.Deg2Rad), Mathf.Cos(angle * Mathf.Deg2Rad));
            Vector3 offset = dir * 1f;

            GameObject proj = Instantiate(skill.skillEffectPrefab, transform.position + offset, skill.skillEffectPrefab.transform.rotation);

            HomingProjectile p = proj.GetComponent<HomingProjectile>();
            if (p != null)
            {
                p.SetTarget(target, skill.damage, skill.projectileSpeed);
            }
        }
    }

    private void UseEnchantSkill(SkillData skill)
    {
        pa.ActivateEnhancedAttack(skill.attackCount, skill.enhancedBonusDamage, skill.skillEffectPrefab);
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
