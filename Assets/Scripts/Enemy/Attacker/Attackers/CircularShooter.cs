using UnityEngine;
using System;

public class CircularShooter : Attacker
{
    [Header("Projectiles")]
    [SerializeField] private int bulletCount = 8;

    [Header("Pattern")]
    [Tooltip("Rotate the circle pattern so one bullet points toward the player.")]
    [SerializeField] private bool alignToPlayer = true;
    [Tooltip("Additional fixed rotation offset on top of player alignment, in degrees.")]
    [SerializeField] [Range(-360f,360f)] private float rotationOffset = 0f;

    protected override void Fire()
    {
        if (bulletCount <= 0) return;

        // Optionally align first bullet toward player
        float baseAngle = rotationOffset;
        if (alignToPlayer && Target != null)
        {
            Vector2 toPlayer = Target.position - transform.position;
            baseAngle += Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;
        }

        float angleStep = 360f / bulletCount;

        for (int i = 0; i < bulletCount; i++)
        {
            float angleDeg = baseAngle + angleStep * i;
            float angleRad = angleDeg * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));

            Vector2 targetPos = (Vector2)transform.position + dir;

            Projectile p = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            p.Initialize(targetPos, projectileSpeed);
        }
    }
}