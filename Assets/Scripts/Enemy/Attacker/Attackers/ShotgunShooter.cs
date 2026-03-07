using UnityEngine;
using System.Collections;
using System;

public class ShotgunShooter : Attacker
{
    [Header("Projectiles")]
    [SerializeField] private int bulletCount = 5;
    [Tooltip("Total spread angle in degrees. e.g. 60 means ±30 around center.")]
    [SerializeField] private float spreadAngle = 60f;

    [Header("Prediction")]
    [Tooltip("Lead the shot by predicting where the player will be. 0 = shoot at current position.")]
    [SerializeField] private float predictionTime = 0f;
    [SerializeField] private float bulletSpeed = 8f; // used for auto-prediction if predictionTime <= 0
    [Tooltip("If true, prediction time is estimated from bullet travel distance / bullet speed automatically.")]
    [SerializeField] private bool autoPrediction = false;

    private Rigidbody2D targetRb;

    private void Awake()
    {
        if (Target != null)
            targetRb = Target.GetComponent<Rigidbody2D>();
    }

    protected override void Fire()
    {
        if (bulletCount <= 0 || Target == null)
        {
            return;
        }

        Vector2 aimTarget = GetAimTarget();
        Vector2 centerDir = (aimTarget - (Vector2)transform.position).normalized;
        float centerAngleDeg = Mathf.Atan2(centerDir.y, centerDir.x) * Mathf.Rad2Deg;

        if (bulletCount == 1)
        {
            SpawnBullet(centerAngleDeg);
            return;
        }

        float halfSpread = spreadAngle / 2f;
        float step = spreadAngle / (bulletCount - 1);

        for (int i = 0; i < bulletCount; i++)
        {
            float offset = -halfSpread + step * i;
            SpawnBullet(centerAngleDeg + offset);
        }
    }

    private void SpawnBullet(float angleDeg)
    {
        float rad = angleDeg * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        Vector2 targetPos = (Vector2)transform.position + dir * 100f;

        Projectile p = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        p.Initialize(targetPos, projectileSpeed);
    }

    private Vector2 GetAimTarget()
    {
        Vector2 playerPos = Target.position;

        if (autoPrediction && targetRb != null)
        {
            float dist = Vector2.Distance(transform.position, playerPos);
            float travelTime = dist / Mathf.Max(bulletSpeed, 0.1f);
            return playerPos + targetRb.linearVelocity * travelTime;
        }

        if (predictionTime > 0f && targetRb != null)
        {
            return playerPos + targetRb.linearVelocity * predictionTime;
        }

        return playerPos;
    }
}