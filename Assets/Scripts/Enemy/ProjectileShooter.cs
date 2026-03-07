public class ProjectileShooter : Attacker
{
    protected override void Fire()
    {
        Projectile projectile = Instantiate(projectilePrefab, transform.position, projectilePrefab.transform.rotation);
        projectile.Initialize(Target.position);
    }
}