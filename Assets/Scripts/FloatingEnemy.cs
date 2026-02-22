using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FloatingEnemy : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    private HealthComponent _healthComponent;
    private IAttacker _attacker;

    [Header("Positioning")]
    [SerializeField] private float cameraOffsetY = 1f;
    [SerializeField] private float playerOffsetX = 2f;

    [Header("Movement")]
    [SerializeField] private float acceleration = 2f;
    [SerializeField] private float maxSpeed = 3f;
    [SerializeField] private float drag = 0.95f;
    [SerializeField] private float catchUpAcceleration = 6f;
    [SerializeField] private float catchUpMaxSpeed = 7f;
    [SerializeField] private float catchUpThreshold = 2f;
    [SerializeField] private float catchUpResolvedThreshold = 0.5f;
    [SerializeField] private float catchUpDuration = 2f;
    private Vector2 _velocity;
    private bool _isCatchingUp;
    private float Acceleration => _isCatchingUp ? catchUpAcceleration : acceleration;
    private float Speed => _isCatchingUp ? catchUpMaxSpeed : maxSpeed;
    private float _startedCatchingUpTime = float.MinValue;

    [Header("Glow")]
    [SerializeField] private Light2D glowLight;
    [SerializeField] private float maxGlowIntensity = 3f;
    private Coroutine _glowRoutine;

    // Other
    private LevelBounds LB => LevelBounds.Instance;
    private float _time = 0f;

    private void Awake()
    {
        _healthComponent = GetComponent<HealthComponent>();
        _attacker = GetComponent<IAttacker>();
    }

    private void OnEnable()
    {
        _healthComponent.HealthDepleted += Die;
        _attacker.ChargeUpStarted += GlowUp;
        _attacker.ChargeDownStarted += GlowDown;
    }

    private void OnDisable()
    {
        _healthComponent.HealthDepleted -= Die;
        _attacker.ChargeUpStarted -= GlowUp;
        _attacker.ChargeDownStarted -= GlowDown;
    }

    private void Start()
    {
        StartAttacking();
    }

    private void Update()
    {
        _time += Time.deltaTime;
        HandleMovement();
    }

    private void HandleMovement()
    {
        float playerSide = player.position.x - LB.MidX;

        float targetX = LB.MidX - playerSide + (playerSide < 0 ? playerOffsetX : -playerOffsetX);
        float targetY = LB.CameraTopY - cameraOffsetY;
        Vector2 target = new Vector2(targetX, targetY);

        Vector2 direction = (target - (Vector2)transform.position).normalized;
        float distanceFromTarget = Vector2.Distance(transform.position, target);

        float distanceFromTargetY = Mathf.Abs(targetY - transform.position.y);
        float displacementFromPlayer = transform.position.y - player.position.y;
        float displacementFromTargetY = transform.position.y - targetY;

        bool belowPlayer = displacementFromPlayer < 0 && -displacementFromPlayer >= catchUpThreshold;
        bool aboveTargetY = displacementFromTargetY > 0 && displacementFromTargetY >= catchUpThreshold;

        if (!_isCatchingUp && (belowPlayer || aboveTargetY))
        {
            StartCatchingUp();
        }

        bool catchUpDurationOver = _time >= _startedCatchingUpTime + catchUpDuration;
        bool caughtUp = distanceFromTargetY <= catchUpResolvedThreshold;

        if (_isCatchingUp && (caughtUp || catchUpDurationOver))
        {
            StopCatchingUp();
        }

        if (distanceFromTarget > 0.1f)
            _velocity += Acceleration * Time.deltaTime * direction;

        _velocity *= drag;
        _velocity = Vector2.ClampMagnitude(_velocity, Speed);
        transform.position += (Vector3)_velocity * Time.deltaTime;
    }

    private void StartCatchingUp()
    {
        _startedCatchingUpTime = _time;
        _isCatchingUp = true;
        StopAttacking();
        StopGlowInstantly();
    }

    private void StopCatchingUp()
    {
        _isCatchingUp = false;
        StartAttacking();
    }

    private void StartAttacking()
    {
        _attacker.StartAttacking();
    }

    private void StopAttacking()
    {
        _attacker.StopAttacking();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Placeholder. Later, check collision with attack hitbox
        if (collision.gameObject.GetComponent<PlayerController>() != null)
        {
            _healthComponent.TakeDamage(25f);
            StartCatchingUp();
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    private void GlowUp(float duration)
    {
        if (_glowRoutine != null)
            StopCoroutine(_glowRoutine);

        _glowRoutine = StartCoroutine(AnimateGlow(glowLight.intensity, maxGlowIntensity, duration));
    }

    private void GlowDown(float duration)
    {
        if (_glowRoutine != null)
            StopCoroutine(_glowRoutine);

        _glowRoutine = StartCoroutine(AnimateGlow(glowLight.intensity, 0f, duration));
    }

    private IEnumerator AnimateGlow(float startIntensity, float endIntensity, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            glowLight.intensity = Mathf.Lerp(startIntensity, endIntensity, t);
            yield return null;
        }

        glowLight.intensity = endIntensity;
        _glowRoutine = null;
    }

    private void StopGlowInstantly()
    {
        if (_glowRoutine != null)
            StopCoroutine(_glowRoutine);

        // Fade down super fast
        _glowRoutine = StartCoroutine(AnimateGlow(glowLight.intensity, 0f, 0.05f));
    }
}