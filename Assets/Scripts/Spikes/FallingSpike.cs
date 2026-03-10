using UnityEngine;

public class FallingSpike : MonoBehaviour
{
    [SerializeField] private float _dmg;
    private bool hasBeenOnScreen = false;
    public float Dmg
    {
        get => _dmg;
        set => _dmg = value;
    }

    // Just player for now
    [SerializeField] private LayerMask targetLayer;

    private SpriteRenderer _sr;
    private LevelBounds LB => LevelBounds.Instance;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        // TODO: make a general deleteOnceOutOfCamera component
        bool onScreen = _sr.Top() > 0 && _sr.Bottom() < LB.CameraTopY
                     && _sr.Right() > LB.CameraLeftX && _sr.Left() < LB.CameraRightX;

        if (onScreen) hasBeenOnScreen = true;

        if (hasBeenOnScreen && !onScreen)
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
                Vector2 direction = (collision.transform.position - transform.position).normalized;
                healthComponent.TakeDamage(Dmg, direction);
            }

            Destroy(gameObject);
        }
    }
}
