using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    // Events
    public event Action<bool, float> GroundedChanged;
    public event Action Jumped;

    // Components
    [Header("Components")]
    [SerializeField] private SpriteRenderer _sr;
    private Rigidbody2D _rb;
    private CapsuleCollider2D _col;

    // Input
    [Header("Input")]
    //[SerializeField] private float horizontalDeadZoneThreshold = 0.1f;
    private bool _jumpDown;
    private bool _jumpHeld;
    private bool _dashDown;
    private float _horizontalMoveDirection;

    // Collision
    [Header("Collision Settings")]
    [SerializeField] private float collisionCheckDistance = 0.05f;
    [SerializeField] private LayerMask playerLayer;
    private float _lastGroundedTime = float.MinValue;
    private bool _grounded;
    private bool _globalQueryStartInColliders;


    // Jump
    [Header("Jump Settings")]
    [SerializeField] private float jumpBuffer = 1.0f;
    [SerializeField] private float coyoteTime = 1.0f;
    [SerializeField] private float jumpPower = 1.0f;
    private bool _jumpToConsume;
    private bool _bufferedJumpUsable;
    private bool _endedJumpEarly;
    private bool _coyoteUsable;
    private float _timeJumpWasPressed;
    private bool HasBufferedJump => _bufferedJumpUsable && _time < _timeJumpWasPressed + jumpBuffer;
    private bool CanUseCoyote => _coyoteUsable && !_grounded && _time < _lastGroundedTime + coyoteTime;

    // Dash
    [Header("Dash")]
    [SerializeField] private float dashBuffer;
    [SerializeField] private float dashSpeed = 12f;
    [SerializeField] private float dashDuration = 0.7f;
    [SerializeField] private float dashCooldown = 1.0f;
    private bool _dashToConsume;
    private bool _bufferedDashUsable;
    private float _timeDashWasPressed;
    private bool HasBufferedDash => _bufferedDashUsable && _time < _timeDashWasPressed + dashBuffer;
    private bool _isDashing = false;
    private bool _dashCooldownEnded = true;
    private bool _touchedGroundAfterDash = false;
    private bool CanDash => _dashCooldownEnded && _touchedGroundAfterDash && !_isDashing;

    // Horizontal Movement
    [Header("Horizontal Movement")]
    [SerializeField] private float groundDeceleration = 22.0f;
    [SerializeField] private float airDeceleration = 1.0f;
    [SerializeField] private float maxHorizontalSpeed = 1.0f;
    [SerializeField] private float acceleration = 100.0f;
    private float _currentDirection;

    // Gravity
    [Header("Gravity")]
    [SerializeField] private float groundingForce = -1.0f;
    [SerializeField] private float fallAcceleration = -10.0f;
    [SerializeField] private float jumpEndEarlyGravityModifier = -2.0f;
    [SerializeField] private float maxFallSpeed = 30.0f;

    private float _time;
    private Vector2 _frameVelocity;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<CapsuleCollider2D>();

        _rb.freezeRotation = true;
        _globalQueryStartInColliders = Physics2D.queriesStartInColliders;
    }

    private void Start()
    {
        if (_currentDirection == 0f)
        {
            _currentDirection = _sr.flipX ? -1f : 1f;
        }   
    }

    private void Update()
    {
        _time += Time.deltaTime;
        GatherInput();
        HandleSpriteFlip();
    }

    private void GatherInput()
    {
        #region jump

        _jumpDown = Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space);
        _jumpHeld = Input.GetButton("Jump") || Input.GetKey(KeyCode.Space);

        if (_jumpDown)
        {
            _jumpToConsume = true;
            _timeJumpWasPressed = _time;
        }

        #endregion

        #region dash

        _dashDown = Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.Mouse3);

        if (_dashDown)
        {
            _dashToConsume = true;
            _timeDashWasPressed = _time;
        }

        #endregion

        #region horizontal movement

        bool leftPressed = Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow);
        bool rightPressed = Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow);

        bool leftHeld = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
        bool rightHeld = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);

        if (leftPressed && rightPressed)
        {
            _horizontalMoveDirection = 1.0f;
        }
        else if (leftPressed)
        {
            _horizontalMoveDirection = -1.0f;
        }
        else if (rightPressed)
        {
            _horizontalMoveDirection = 1.0f;
        }
        else if (leftHeld && rightHeld)
        {
            // Both held from before, so it keeps the last direction
        }
        else if (leftHeld)
        {
            _horizontalMoveDirection = -1.0f;
        }
        else if (rightHeld)
        {
            _horizontalMoveDirection = 1.0f;
        }
        else
        {
            // Nothing held
            _horizontalMoveDirection = 0.0f;
        }

        if (_horizontalMoveDirection != 0.0f) _currentDirection = _horizontalMoveDirection;

        #endregion
    }

    private void FixedUpdate()
    {
        CheckCollisions();

        if (!_isDashing)
        {
            // TODO: enable dash interruption with jump later
            HandleJump();
            HandleDirection();
            HandleGravity();
            HandleDash();
        }
        ApplyMovement();
    }

    private void CheckCollisions()
    {
        Physics2D.queriesStartInColliders = false;

        // Ground and Ceiling
        bool groundHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.down, collisionCheckDistance, ~playerLayer);
        bool ceilingHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.up, collisionCheckDistance, ~playerLayer);

        // Hit a Ceiling
        if (ceilingHit) _frameVelocity.y = Mathf.Min(0, _frameVelocity.y);

        // Landed on the Ground
        if (!_grounded && groundHit)
        {
            _grounded = true;
            _coyoteUsable = true;
            _bufferedJumpUsable = true;
            _endedJumpEarly = false;
            GroundedChanged?.Invoke(true, Mathf.Abs(_frameVelocity.y));
        }
        // Left the Ground
        else if (_grounded && !groundHit)
        {
            _grounded = false;
            _lastGroundedTime = _time;
            GroundedChanged?.Invoke(false, 0);
        }

        if (_grounded) _touchedGroundAfterDash = true;

        Physics2D.queriesStartInColliders = _globalQueryStartInColliders;
    }

    private void HandleJump()
    {
        // Ended jump/released jump button while going up
        if (!_endedJumpEarly && !_grounded && !_jumpHeld && _rb.linearVelocity.y > 0) _endedJumpEarly = true;

        // If no jumps to execute
        if (!_jumpToConsume && !HasBufferedJump) return;

        if (_grounded || CanUseCoyote) ExecuteJump();

        _jumpToConsume = false;
    }

    private void ExecuteJump()
    {
        _endedJumpEarly = false;
        _timeJumpWasPressed = 0;
        _bufferedJumpUsable = false;
        _coyoteUsable = false;
        _frameVelocity.y = jumpPower;
        Jumped?.Invoke();
    }

    private void HandleDirection()
    {
        if (_horizontalMoveDirection == 0)
        {
            float deceleration = _grounded ? groundDeceleration : airDeceleration;
            _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, deceleration * Time.fixedDeltaTime);
        }
        else
        {
            float targetVelocity = _horizontalMoveDirection * maxHorizontalSpeed;
            _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, targetVelocity, acceleration * Time.fixedDeltaTime);
        }
    }

    private void HandleGravity()
    {
        if (_grounded && _frameVelocity.y <= 0f)
        {
            _frameVelocity.y = groundingForce;
        }
        else
        {
            float inAirGravity = fallAcceleration;
            bool shouldFall = _endedJumpEarly && _frameVelocity.y > 0;
            if (shouldFall) inAirGravity *= jumpEndEarlyGravityModifier;

            _frameVelocity.y = Mathf.MoveTowards(_frameVelocity.y, -maxFallSpeed, inAirGravity * Time.fixedDeltaTime);
        }
    }

    private void HandleDash()
    {
        if (!_dashToConsume && !HasBufferedDash) return;
        _dashToConsume = false;
        if (!CanDash) return;

        ExecuteDash();
    }

    private void ExecuteDash()
    {
        _frameVelocity = Mathf.Sign(_currentDirection) * dashSpeed * Vector2.right;

        _isDashing = true;
        _touchedGroundAfterDash = false;
        _dashCooldownEnded = false;

        StartCoroutine(DashRoutine());

        IEnumerator DashRoutine()
        {
            yield return new WaitForSeconds(dashDuration);

            _isDashing = false;
            _frameVelocity = Vector2.zero;
            StartCoroutine(DashCooldownRoutine());
        }

        IEnumerator DashCooldownRoutine()
        {
            yield return new WaitForSeconds(dashCooldown);
            _dashCooldownEnded = true;
        }
    }

private void ApplyMovement()
    {
        _rb.linearVelocity = _frameVelocity;
    }

    private void HandleSpriteFlip()
    {
        if (_horizontalMoveDirection > 0f)
        {
            _sr.flipX = false;
        }
        else if (_horizontalMoveDirection < 0f)
        {
            _sr.flipX = true;
        }
    }
}


