using UnityEngine;
using System.Collections;

public class ProjectileShooter : MonoBehaviour, IAttacker
{
    [Header("Projectiles")]
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private float shootProjectilesInterval = 5.0f;
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
            ShootProjectile();
            yield return new WaitForSeconds(shootProjectilesInterval);
        }
    }

    private void ShootProjectile()
    {
        Projectile projectile = Instantiate(projectilePrefab, transform.position, projectilePrefab.transform.rotation);
        projectile.Initialize(target.position);
    }

}
