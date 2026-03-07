using UnityEngine;
using System;
using System.Collections;

public abstract class Attacker : MonoBehaviour, IAttacker
{
    public event Action<float> ChargeUpStarted;
    public event Action<float> ChargeDownStarted;
    public event Action<float> CooldownStarted;

    [Header("Projectile")]
    [SerializeField] protected Projectile projectilePrefab;
    [SerializeField] protected float projectileSpeed;

    [Header("Timing")]
    [SerializeField] protected float chargeUpDuration = 1f;
    [SerializeField] protected float chargeDownDuration = 0.2f;
    [SerializeField] protected float cooldownDuration = 1.5f;

    protected float speedMultiplier = 1f;

    protected float ChargeUp => chargeUpDuration / speedMultiplier;
    protected float ChargeDown => chargeDownDuration / speedMultiplier;
    protected float Cooldown => cooldownDuration / speedMultiplier;

    public Transform Target { get; set; }

    protected Coroutine attackRoutine;
    protected bool isAttacking;

    public void StartAttacking()
    {
        if (isAttacking) return;
        isAttacking = true;
        attackRoutine = StartCoroutine(AttackCoroutine());
    }

    public void StopAttacking()
    {
        if (!isAttacking) return;
        isAttacking = false;
        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }
    }

    protected abstract void Fire(); 

    protected virtual IEnumerator AttackCoroutine()
    {
        while (true)
        {
            ChargeUpStarted?.Invoke(ChargeUp);
            yield return new WaitForSeconds(ChargeUp);

            Fire();

            ChargeDownStarted?.Invoke(ChargeDown);
            yield return new WaitForSeconds(ChargeDown);

            CooldownStarted?.Invoke(Cooldown);
            yield return new WaitForSeconds(Cooldown);
        }
    }

    public void SetAttackSpeedMultiplier(float multiplier)
    {
        speedMultiplier = Mathf.Max(0.01f, multiplier);
        if (isAttacking)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = StartCoroutine(AttackCoroutine());
        }
    }
}
