using UnityEngine;
using UnityEngine.AdaptivePerformance;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float damage = 10f;
    [SerializeField] private float lifetime = 5f;

    private Vector2 direction;
    private float speed;

    public void Initialize(Vector2 targetPosition, float speed)
    {
        this.speed = speed;
        direction = (targetPosition - (Vector2)transform.position).normalized;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle + 90f);

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.Translate(speed * Time.deltaTime * direction, Space.World);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<HealthComponent>(out var healthComponent))
        {
            Vector2 direction = (other.transform.position - transform.position).normalized;
            healthComponent.TakeDamage(damage, direction);
        }
        Destroy(gameObject);
    }
}