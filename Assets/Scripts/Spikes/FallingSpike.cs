using UnityEngine;

public class FallingSpike : MonoBehaviour
{
    [SerializeField] private float _dmg;
    [SerializeField] private LayerMask targetLayer;

    public float Dmg { get => _dmg; set => _dmg = value; }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (targetLayer.Contains(collision.gameObject.layer))
        {
            if (collision.TryGetComponent<HealthComponent>(out var healthComponent))
            {
                Vector2 direction = (collision.transform.position - transform.position).normalized;
                healthComponent.TakeDamage(_dmg, direction);
            }
            Destroy(gameObject);
        }
    }
}