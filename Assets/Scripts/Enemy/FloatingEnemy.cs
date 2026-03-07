using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

// TODO: refactor with a state machine

public class FloatingEnemy : MonoBehaviour
{
    [Header("References")]
    public  Transform player;
    private HealthComponent healthComponent;
    private SpriteRenderer sr;
    private SwitchableAttacker attacker;
    private DisintegrationEffect disintegrationEffect;

    [Header("Positioning")]
    [SerializeField] private float cameraOffsetY = 1f;
    [SerializeField] private float playerOffsetX = 2f;

    [Header("Movement")]
    [SerializeField] private float acceleration = 2f;
    [SerializeField] private float maxSpeed = 3f;
    [SerializeField] private float caughtUpSpeed = 10f;
    [SerializeField] private float fastAcceleration = 6f;
    [SerializeField] private float fastMaxSpeed = 7f;
    [SerializeField] private float catchUpThreshold = 2f;
    [SerializeField] private float catchUpResolvedThreshold = 0.5f;
    [SerializeField] private float catchUpDuration = 2f;
    private Vector2 _velocity;
    private bool _isCatchingUp;
    private bool _isMovingFast;
    private float Acceleration => _isMovingFast ? fastAcceleration : acceleration;
    private float MaxSpeed => _isMovingFast ? fastMaxSpeed : maxSpeed;
    private float _startedCatchingUpTime = float.MinValue;

    [Header("Glow")]
    [SerializeField] private Light2D glowLight;
    [SerializeField] private float maxGlowIntensity = 3f;
    private Coroutine _glowRoutine;

    [Header("Retaliation")]
    [SerializeField] private float retaliationAttackSpeedMultiplier = 1.5f;
    private float _retaliationDuration;
    private bool _isRetaliating;
    private float _startedRetaliatingTime;
    private bool retaliationSideRight = true;

    // Other
    private LevelBounds LB => LevelBounds.Instance;
    private float _time = 0f;

    private void Awake()
    {
        healthComponent = GetComponent<HealthComponent>();
        attacker = GetComponent<SwitchableAttacker>();
        sr = GetComponentInChildren<SpriteRenderer>();
        disintegrationEffect = GetComponentInChildren<DisintegrationEffect>();
    }

    private void Start()
    {
        _retaliationDuration = healthComponent.InvincibilityDuration;
        StartAttacking();
    }

    private void OnEnable()
    {
        GameManager.GameStateChanged += OnGameStateChanged;
        healthComponent.HealthDepleted += Die;
        attacker.ChargeUpStarted += GlowUp;
        attacker.ChargeDownStarted += GlowDown;
        attacker.CooldownStarted += OnCooldownStarted;
    }

    private void OnDisable()
    {
        GameManager.GameStateChanged -= OnGameStateChanged;
        healthComponent.HealthDepleted -= Die;
        attacker.ChargeUpStarted -= GlowUp;
        attacker.ChargeDownStarted -= GlowDown;
        attacker.CooldownStarted -= OnCooldownStarted;
    }

    private void Update()
    {
        _time += Time.deltaTime;
        HandleMovement();

        if (_isRetaliating && _time >= _startedRetaliatingTime + _retaliationDuration)
        {
            StopRetaliation();
        }

        FlipX();
    }

    private void HandleMovement()
    {
        // TODO: add targeting strategies instead or something. Just refactor to an FSM first
        float targetX;
        float targetY = LB.CameraTopY - cameraOffsetY;

        if (_isRetaliating)
        {
            float offset = retaliationSideRight ? playerOffsetX : -playerOffsetX;
            targetX = LB.MidX + offset;
        }
        else
        {
            float playerSide = player.position.x - LB.MidX;
            targetX = LB.MidX - playerSide + (playerSide < 0 ? playerOffsetX : -playerOffsetX);
        }
        Vector2 target = new Vector2(targetX, targetY);

        // Get distance
        Vector2 direction = (target - (Vector2)transform.position).normalized;
        float distanceFromTarget = Vector2.Distance(transform.position, target);

        float distanceFromTargetY = Mathf.Abs(targetY - transform.position.y);
        float displacementFromPlayer = transform.position.y - player.position.y;
        float displacementFromTargetY = transform.position.y - targetY;

        bool belowPlayer = displacementFromPlayer < 0 && -displacementFromPlayer >= catchUpThreshold;
        bool aboveTargetY = displacementFromTargetY > 0 && displacementFromTargetY >= catchUpThreshold;

        if (!_isCatchingUp && !_isRetaliating && (belowPlayer || aboveTargetY))
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

        _velocity = Vector2.ClampMagnitude(_velocity, MaxSpeed);
        transform.position += (Vector3)_velocity * Time.deltaTime;
    }

    private void StartMovingFast()
    {
        _isMovingFast = true;
    }

    private void StopMovingFast()
    {
        _velocity = Vector2.ClampMagnitude(_velocity, caughtUpSpeed);
        _isMovingFast = false;
    }

    private void StartCatchingUp()
    {
        if (_isCatchingUp) return;
        _startedCatchingUpTime = _time;
        _isCatchingUp = true;
        StopAttacking();
        StopGlowInstantly();
        StartMovingFast(); 
        healthComponent.AddInvincibleEffect();
    }

    private void StopCatchingUp()
    {
        if (!_isCatchingUp) return;
        _isCatchingUp = false;
        StartAttacking();
        StopMovingFast();
        healthComponent.RemoveInvincibleEffect();
    }

    private void StartAttacking()
    {
        attacker.StartAttacking();
    }

    private void StopAttacking()
    {
        attacker.StopAttacking();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (healthComponent.IsInvincible) return;

        // TODO: detect player in another way
        if (collision.gameObject.GetComponent<PlayerStateDriver>() != null)
        {
            Vector2 direction = (collision.transform.position - transform.position).normalized;
            healthComponent.TakeDamage(25f, direction);
            StartRetaliation();
        }
    }

    private void StartRetaliation()
    {
        StopCatchingUp();

        _isRetaliating = true;
        _startedRetaliatingTime = _time;

        StopAttacking();
        attacker.SwitchTo<CircularShooter>();
        attacker.SetAttackSpeedMultiplier(retaliationAttackSpeedMultiplier);
        StartAttacking();

        StartMovingFast();
    }

    private void StopRetaliation()
    {
        _isRetaliating = false;
        attacker.SetAttackSpeedMultiplier(1f);

        if (healthComponent.HealthPercentage <= 0.5)
        {
            attacker.SwitchTo<ShotgunShooter>();
        }
        else
        {
            attacker.SwitchTo<ProjectileShooter>();
        } 

        StopMovingFast();
        StartAttacking();
    }

    private void Die()
    {
        disintegrationEffect.Disintegrate();
        Destroy(gameObject);
    }

    // TODO: make a glowing component
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

    // TODO: refactor to a general 'Look at' component or whatever that looks at a target
    private void FlipX()
    {
        sr.flipX = player.transform.position.x < transform.position.x;
    }

    private void OnCooldownStarted(float duration)
    {
        if (_isRetaliating)
            retaliationSideRight = !retaliationSideRight;
    }

    private void OnGameStateChanged(GameManager.GameState gameState)
    {
        if (gameState == GameManager.GameState.Dead)
        {
            enabled = false;
            StopAttacking();
            attacker.enabled = false;
        }
    }
}