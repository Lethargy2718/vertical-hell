using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    // Events
    public event Action<bool, float> GroundedChanged;
    public event Action Jumped;
    public event Action Dashed;
    public event Action DashEnded;

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
    private Vector2 _inputVec;

    // Collision
    [Header("Collision Settings")]
    [SerializeField] private float collisionCheckDistance = 0.05f;
    [SerializeField] private LayerMask excludeFromCollisions;
    private float _lastGroundedTime = float.MinValue;
    private bool _grounded;
    private bool _globalQueryStartInColliders;


    // Jump
    [Header("Jump Settings")]
    [SerializeField] private float jumpBuffer = 1.0f;
    [SerializeField] private float coyoteTime = 1.0f;

    [SerializeField]
    [Tooltip("In world units")]
    private float jumpHeight = 2.0f;

    private bool _jumpToConsume;
    private bool _hasBufferedJump;
    private bool _endedJumpEarly;
    private bool _coyoteUsable;
    private float _timeJumpWasPressed = float.MinValue;
    private bool CanUseBufferedJump => _hasBufferedJump && _time < _timeJumpWasPressed + jumpBuffer;
    private bool CanUseCoyote => _coyoteUsable && !_grounded && _time < _lastGroundedTime + coyoteTime;
    private float JumpForce => Mathf.Sqrt(2f * fallAcceleration * jumpHeight) + (fallAcceleration * Time.fixedDeltaTime / 2f);

    // Dash
    [Header("Dash")]
    [SerializeField] private float dashBuffer;
    [SerializeField] private float dashSpeed = 12f;
    [SerializeField] private float dashDuration = 0.7f;
    [SerializeField] private float dashCooldown = 1.0f;
    private bool _dashToConsume;
    private bool _hasBufferedDash;
    private float _timeDashWasPressed = float.MinValue;
    private bool CanUseBufferedDash => _hasBufferedDash && _time < _timeDashWasPressed + dashBuffer;
    private bool _isDashing = false;
    private bool _dashCooldownEnded = true;
    private bool _touchedGroundAfterDash = false;
    private bool CanDash => _dashCooldownEnded && _touchedGroundAfterDash && !_isDashing;
    private Coroutine dashCoroutine;
    private float _currentVerticalDirection;

    // Horizontal Movement
    [Header("Horizontal Movement")]
    [SerializeField] private float groundDeceleration = 22.0f;
    [SerializeField] private float airDeceleration = 1.0f;
    [SerializeField] private float maxHorizontalSpeed = 1.0f;
    [SerializeField] private float acceleration = 100.0f;
    private float _currentHorizontalDirection;

    // Gravity
    [Header("Gravity")]
    [SerializeField] private float groundingForce = -1.0f;
    [SerializeField] private float fallAcceleration = 35.0f;
    [SerializeField] private float jumpEndEarlyGravityModifier = 4.0f;
    [SerializeField] private float maxFallSpeed = 30.0f;

    private float _time;
    private Vector2 _frameVelocity;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<CapsuleCollider2D>();

        _rb.freezeRotation = true;
        _rb.gravityScale = 0;
        _globalQueryStartInColliders = Physics2D.queriesStartInColliders;
    }

    private void Start()
    {
        if (_currentHorizontalDirection == 0f)
        {
            _currentHorizontalDirection = _sr.flipX ? -1f : 1f;
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
        #region Jump Input

        _jumpDown = Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space);
        _jumpHeld = Input.GetButton("Jump") || Input.GetKey(KeyCode.Space);

        if (_jumpDown)
        {
            _jumpToConsume = true;
            _timeJumpWasPressed = _time;
            _hasBufferedJump = true;
        }

        #endregion

        #region Dash Input

        _dashDown = Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.Mouse3);

        if (_dashDown)
        {
            _hasBufferedDash = true;
            _dashToConsume = true;
            _timeDashWasPressed = _time;
        }

        #endregion

        #region Horizontal Input

        bool leftPressed = Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow);
        bool rightPressed = Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow);

        bool leftHeld = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
        bool rightHeld = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);

        _inputVec.x = HandleInputPriority(leftPressed, rightPressed, leftHeld, rightHeld, _inputVec.x);
        if (_inputVec.x != 0.0f) _currentHorizontalDirection = _inputVec.x;

        #endregion

        #region Vertical Input

        bool downPressed = Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow);
        bool upPressed = Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);

        bool downHeld = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
        bool upHeld = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);


        _inputVec.y = HandleInputPriority(downPressed, upPressed, downHeld, upHeld, _inputVec.y);
        if (_inputVec.y != 0.0f) _currentVerticalDirection = _inputVec.y;

        #endregion
    }

    private float HandleInputPriority(bool pressed1, bool pressed2, bool held1, bool held2, float oldVal)
    {
        if (pressed1 && pressed2) return 1.0f;
        if (pressed1) return -1.0f;
        if (pressed2) return 1.0f;
        if (held1 && held2) return oldVal;
        if (held1) return -1.0f;
        if (held2) return 1.0f;
        return 0.0f;
    }

    private void FixedUpdate()
    {
        CheckCollisions();
        HandleJump();

        if (!_isDashing)
        {
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
        bool groundHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.down, collisionCheckDistance, ~excludeFromCollisions);
        bool ceilingHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.up, collisionCheckDistance, ~excludeFromCollisions);

        // Hit a Ceiling
        if (ceilingHit) _frameVelocity.y = Mathf.Min(0, _frameVelocity.y);

        // Landed on the Ground
        if (!_grounded && groundHit)
        {
            _grounded = true;
            _coyoteUsable = true;
            //_bufferedJumpUsable = true;
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
        if (!(_jumpToConsume || CanUseBufferedJump)) return;
        if (_grounded || CanUseCoyote)
        {
            if (_isDashing)
            {
                StopCoroutine(dashCoroutine);
                EndDash();
            }
            ExecuteJump();
        }

        _jumpToConsume = false;
    }

    private void ExecuteJump()
    {
        _endedJumpEarly = false;
        _timeJumpWasPressed = 0;
        _hasBufferedJump = false;
        _coyoteUsable = false;
        _frameVelocity.y = JumpForce;
        Jumped?.Invoke();
    }

    private void HandleDirection()
    {
        if (_inputVec.x == 0)
        {
            float deceleration = _grounded ? groundDeceleration : airDeceleration;
            _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, deceleration * Time.fixedDeltaTime);
        }
        else
        {
            float targetVelocity = _inputVec.x * maxHorizontalSpeed;
            _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, targetVelocity, acceleration * Time.fixedDeltaTime);
        }
    }

    private void HandleGravity()
    {
        if (_grounded && _frameVelocity.y <= 0f)
        {
            _frameVelocity.y = groundingForce;
            return;
        }

        float gravityThisFrame = fallAcceleration * Time.fixedDeltaTime;

        if (_endedJumpEarly && _frameVelocity.y > 0)
        {
            gravityThisFrame *= jumpEndEarlyGravityModifier;
        }

        _frameVelocity.y += -gravityThisFrame;
        _frameVelocity.y = Mathf.Max(_frameVelocity.y, -maxFallSpeed);
    }

    private void HandleDash()
    {
        if (!_dashToConsume && !CanUseBufferedDash) return;
        _dashToConsume = false;
        if (!CanDash) return;

        ExecuteDash();
    }

    private void ExecuteDash()
    {
        Vector2 dashDirection;
        if (_inputVec.x == 0f && _inputVec.y != 0f)
        {
            dashDirection = _inputVec;
        }
        else dashDirection = new Vector2(_currentHorizontalDirection, _inputVec.y).normalized;

        _frameVelocity = dashSpeed * dashDirection;

        _isDashing = true;
        _touchedGroundAfterDash = false;
        _dashCooldownEnded = false;
        _hasBufferedDash = false;
        Dashed?.Invoke();
        dashCoroutine = StartCoroutine(DashRoutine());

        IEnumerator DashRoutine()
        {
            yield return new WaitForSeconds(dashDuration);
            EndDash();
        }
    }

    private void EndDash()
    {
        _isDashing = false;
        _frameVelocity = new Vector2(0, _frameVelocity.y);
        DashEnded?.Invoke();
        StartCoroutine(DashCooldownRoutine());

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
        if (_inputVec.x > 0f)
        {
            _sr.flipX = false;
        }
        else if (_inputVec.x < 0f)
        {
            _sr.flipX = true;
        }
    }
}


