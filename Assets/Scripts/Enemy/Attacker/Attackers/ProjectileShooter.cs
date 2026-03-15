using UnityEngine;

public class ProjectileShooter : Attacker
{
    [Header("Prediction")]
    [SerializeField] private bool autoPrediction = false;
    [SerializeField] private float predictionTime = 0f;

    private Rigidbody2D targetRb;

    private void Start()
    {
        targetRb = Target.GetComponent<Rigidbody2D>();
    }

    protected override void Fire()
    {
        Projectile projectile = SpawnProjectile();
        projectile.Initialize(GetAimTarget(), projectileSpeed);
    }

    // TODO: move up to Attacker potentially. same code as shotgun
    private Vector2 GetAimTarget()
    {
        Vector2 playerPos = Target.position;

        if (autoPrediction && targetRb != null)
        {
            float dist = Vector2.Distance(transform.position, playerPos);
            float travelTime = dist / Mathf.Max(projectileSpeed, 0.1f);
            return playerPos + targetRb.linearVelocity * travelTime;
        }

        if (predictionTime > 0f && targetRb != null)
            return playerPos + targetRb.linearVelocity * predictionTime;

        return playerPos;
    }
}