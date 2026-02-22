using UnityEngine;
using System.Collections;
using System;

public class ProjectileShooter : MonoBehaviour, IAttacker
{
    public event Action<float> ChargeUpStarted;
    public event Action<float> ChargeDownStarted;
    public event Action<float> CooldownStarted;

    [Header("Projectiles")]
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private float chargeUpDuration = 1f;
    [SerializeField] private float chargeDownDuration = 1f;
    [SerializeField] private float cooldownDuration = 1f;

    private float _speedMultiplier = 1f;
    private float ChargeUp => chargeUpDuration / _speedMultiplier;
    private float ChargeDown => chargeDownDuration / _speedMultiplier;
    private float Cooldown => cooldownDuration / _speedMultiplier;

    private Coroutine _shootProjectilesRoutine;
    private bool _isAttacking = false;
    public Transform target;

    public void StartAttacking()
    {
        if (_isAttacking) return;
        _isAttacking = true;
        _shootProjectilesRoutine = StartCoroutine(ShootProjectilesCoroutine());
    }

    public void StopAttacking()
    {
        if (!_isAttacking) return;
        _isAttacking = false;
        StopCoroutine(_shootProjectilesRoutine);
        _shootProjectilesRoutine = null;
    }

    public void SetAttackSpeedMultiplier(float multiplier)
    {
        _speedMultiplier = Mathf.Max(0.01f, multiplier);

        if (_isAttacking)
        {
            StopCoroutine(_shootProjectilesRoutine);
            _shootProjectilesRoutine = StartCoroutine(ShootProjectilesCoroutine());
        }
    }

    private IEnumerator ShootProjectilesCoroutine()
    {
        while (true)
        {
            ChargeUpStarted?.Invoke(ChargeUp);
            yield return new WaitForSeconds(ChargeUp);
            ShootProjectile();
            ChargeDownStarted?.Invoke(ChargeDown);
            yield return new WaitForSeconds(ChargeDown);
            CooldownStarted?.Invoke(Cooldown);
            yield return new WaitForSeconds(Cooldown);
        }
    }

    private void ShootProjectile()
    {
        Projectile projectile = Instantiate(projectilePrefab, transform.position, projectilePrefab.transform.rotation);
        projectile.Initialize(target.position);
    }
}