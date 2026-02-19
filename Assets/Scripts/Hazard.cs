using UnityEngine;

public class Hazard : MonoBehaviour
{
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float dps = 10f;

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent<HealthComponent>(out var healthComponent))
        {
            healthComponent.Health -= dps * Time.fixedDeltaTime;
        }
    }
}
