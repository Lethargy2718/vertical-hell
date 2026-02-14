using System;
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
    private float _horizontalDirection;

    // Collision
    [Header("Collision Settings")]
    [SerializeField] private float collisionCheckDistance = 0.05f;
    [SerializeField] private LayerMask playerLayer;
    private float _lastGroundedTime = float.MinValue;
    private bool _grounded;
    private bool _globalQueryStartInColliders;


    // Jumps
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

    // Horizontal Movement
    [Header("Horizontal Movement")]
    [SerializeField] private float groundDeceleration = 22.0f;
    [SerializeField] private float airDeceleration = 1.0f;
    [SerializeField] private float maxHorizontalSpeed = 1.0f;
    [SerializeField] private float acceleration = 100.0f;

    // Gravity
    [Header("Gravity")]
    [SerializeField] private float groundingForce = -1.0f;
    [SerializeField] private float fallAcceleration = -10.0f;
    [SerializeField] private float jumpEndEarlyGravityModifier = -2.0f;
    [SerializeField] private float maxFallSpeed = 30.0f;

    // Shaders
    [Header("Shaders")]
    [SerializeField] private Material _spriteMaterial;
    private const string FLIP_X = "_FlipX";

    private float _time;
    private Vector2 _frameVelocity;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<CapsuleCollider2D>();

        _rb.freezeRotation = true;
        _globalQueryStartInColliders = Physics2D.queriesStartInColliders;
    }

    private void Update()
    {
        _time += Time.deltaTime;
        GatherInput();
        HandleSpriteFlip();
    }

    private void GatherInput()
    {

        _jumpDown = Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space);
        _jumpHeld = Input.GetButton("Jump") || Input.GetKey(KeyCode.Space);

        bool leftPressed = Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow);
        bool rightPressed = Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow);

        bool leftHeld = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
        bool rightHeld = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);

        if (leftPressed && rightPressed)
        {
            _horizontalDirection = 1.0f;
        }
        else if (leftPressed)
        {
            _horizontalDirection = -1.0f;
        }
        else if (rightPressed)
        {
            _horizontalDirection = 1.0f;
        }
        else if (leftHeld && rightHeld)
        {
            // Both held from before, so it keeps the last direction
        }
        else if (leftHeld)
        {
            _horizontalDirection = -1.0f;
        }
        else if (rightHeld)
        {
            _horizontalDirection = 1.0f;
        }
        else
        {
            // Nothing held
            _horizontalDirection = 0.0f;
        }

        if (_jumpDown)
        {
            _jumpToConsume = true;
            _timeJumpWasPressed = _time;
        }
    }

    private void FixedUpdate()
    {
        CheckCollisions();

        HandleJump();
        HandleDirection();
        HandleGravity();
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
        if (_horizontalDirection == 0)
        {
            float deceleration = _grounded ? groundDeceleration : airDeceleration;
            _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, deceleration * Time.fixedDeltaTime);
        }
        else
        {
            float targetVelocity = _horizontalDirection * maxHorizontalSpeed;
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

    private void ApplyMovement()
    {
        _rb.linearVelocity = _frameVelocity;
    }

    private void HandleSpriteFlip()
    {
        if (_horizontalDirection > 0f)
        {
            _spriteMaterial.SetFloat("_FlipX", 0);
        }
        else if (_horizontalDirection < 0f)
        {
            _spriteMaterial.SetFloat("_FlipX", 1);
        }
    }
}


