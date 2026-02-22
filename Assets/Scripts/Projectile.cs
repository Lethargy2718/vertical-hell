using UnityEngine;
using UnityEngine.AdaptivePerformance;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float lifetime = 5f;

    private Vector2 _direction;

    public void Initialize(Vector2 targetPosition)
    {
        _direction = (targetPosition - (Vector2)transform.position).normalized;

        float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle + 90f);

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.Translate(speed * Time.deltaTime * _direction, Space.World);
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