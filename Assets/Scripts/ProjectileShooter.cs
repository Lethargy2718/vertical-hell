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
    //[SerializeField] private float shootProjectilesInterval = 5.0f;
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

    private IEnumerator ShootProjectilesCoroutine()
    {
        while (true)
        {
            ChargeUpStarted?.Invoke(chargeUpDuration);
            yield return new WaitForSeconds(chargeUpDuration);

            ShootProjectile();

            ChargeDownStarted?.Invoke(chargeDownDuration);
            yield return new WaitForSeconds(chargeDownDuration);

            CooldownStarted?.Invoke(cooldownDuration);
            yield return new WaitForSeconds(cooldownDuration);
        }
    }

    private void ShootProjectile()
    {
        Projectile projectile = Instantiate(projectilePrefab, transform.position, projectilePrefab.transform.rotation);
        projectile.Initialize(target.position);
    }

}
