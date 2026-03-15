using UnityEngine;
using System.Collections;

public class Projectile : Flyweight
{
    private new ProjectileSettings settings => (ProjectileSettings)base.settings;

    private Vector2 direction;
    private float speed;

    public void Initialize(Vector2 targetPosition, float speed)
    {
        this.speed = speed;
        direction = (targetPosition - (Vector2)transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle + 90f);
        StartCoroutine(DespawnAfterDelay(settings.lifetime));
    }

    private void Update()
    {
        transform.Translate(speed * Time.deltaTime * direction, Space.World);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<HealthComponent>(out var healthComponent))
        {
            Vector2 dir = (other.transform.position - transform.position).normalized;
            healthComponent.TakeDamage(settings.damage, dir);
        }
        FlyweightFactory.ReturnToPool(this);
    }

    private IEnumerator DespawnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        FlyweightFactory.ReturnToPool(this);
    }
}