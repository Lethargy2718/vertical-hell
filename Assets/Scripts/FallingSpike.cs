using UnityEngine;

public class FallingSpike : MonoBehaviour
{
    [SerializeField] private float _dmg;
    public float Dmg
    {
        get => _dmg;
        set => _dmg = value;
    }

    // Just player for now
    [SerializeField] private LayerMask targetLayer;

    private SpriteRenderer _sr;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (_sr.Top() <= 0)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (targetLayer.Contains(collision.gameObject.layer))
        {
            if (collision.TryGetComponent<HealthComponent>(out var healthComponent))
            {
                healthComponent.TakeDamage(Dmg);
            }

            Destroy(gameObject);
        }
    }
}
