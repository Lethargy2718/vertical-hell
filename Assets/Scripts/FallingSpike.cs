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

    private void Update()
    {
        if (transform.position.y < -transform.localScale.y)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (targetLayer.Contains(collision.gameObject.layer))
        {
            if (collision.TryGetComponent<HealthComponent>(out var healthComponent))
            {
                healthComponent.Health -= Dmg;
            }

            Destroy(gameObject);
        }
    }

}
