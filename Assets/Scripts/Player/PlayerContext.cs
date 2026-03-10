using System;
using Cinemachine;
using UnityEngine;

[Serializable]
public class PlayerContext
{
    // Components
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public CapsuleCollider2D col;
    [HideInInspector] public SpriteRenderer sr;
    [HideInInspector] public bool globalQueryStartInColliders;
    public CinemachineVirtualCamera virtualCamera;

    // Events
    public event Action<bool, float> GroundedChanged;
    public event Action Jumped;
    public event Action Dashed;
    public event Action DashEnded;
    public event Action FlyStarted;
    public event Action FlyEnded;
    public event Action GroundSlamStarted;
    public event Action GroundSlamEnded;

    // Time
    [HideInInspector] public float time;

    // Input
    [HideInInspector] public Vector2 inputVec;
    [HideInInspector] public bool jumpPressed;
    [HideInInspector] public bool jumpHeld;
    [HideInInspector] public bool dashPressed;

    // Movement
    [HideInInspector] public Vector2 frameVelocity;
    [HideInInspector] public float currentHorizontalDirection;
    [HideInInspector] public float currentVerticalDirection;

    // Collision
    [HideInInspector] public bool grounded;
    [HideInInspector] public float lastGroundedTime = float.MinValue;

    // Jump
    [HideInInspector] public bool hasBufferedJump;
    [HideInInspector] public bool endedJumpEarly;
    [HideInInspector] public bool coyoteUsable;
    [HideInInspector] public float timeJumpWasPressed = float.MinValue;

    // Dash
    [HideInInspector] public bool dashToConsume;
    [HideInInspector] public bool hasBufferedDash;
    [HideInInspector] public float timeDashEnded = float.MinValue;
    [HideInInspector] public float timeDashWasPressed = float.MinValue;

    // Ground Slam

    // Fly
    [HideInInspector] public float flyTimeUsed = 0f;

    // Horizontal Movement Parameters
    [Header("Horizontal Movement")]
    public float maxHorizontalSpeed = 6f;
    public float acceleration = 100f;
    public float groundDeceleration = 22f;
    public float airDeceleration = 1f;

    // Jump Parameters
    [Header("Jump")]
    public float jumpHeight = 2f;
    public float coyoteTime = 0.15f;
    public float jumpBuffer = 0.15f;
    public float jumpEndEarlyGravityModifier = 4f;

    // Gravity Parameters
    [Header("Gravity")]
    public float fallAcceleration = 35f;
    public float maxFallSpeed = 30f;
    public float groundingForce = -1f;

    // Dash Parameters
    [Header("Dash")]
    public float dashSpeed = 12f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 1f;
    public float dashBuffer = 0.15f;

    // Fly Parameters
    [Header("Fly")]
    public float maxFlyDuration = float.MaxValue;
    public float flySpeed = 10f;
    public float flyDeceleration = 1f;
    public float cameraOffset = 1f;
    public float cameraOffsetInDuration = 0.5f;
    public float cameraOffsetOutDuration = 0.5f;

    // Ground Slam Parameters
    [Header("Ground Slam")]
    public float groundSlamSpeed = 60f;

    // Helpers
    public bool CanUseBufferedJump => hasBufferedJump && time < timeJumpWasPressed + jumpBuffer;
    public bool CanUseCoyote => coyoteUsable && !grounded && time < lastGroundedTime + coyoteTime;
    public bool CanUseBufferedDash => hasBufferedDash
        && time < timeDashWasPressed + dashBuffer
        && time > timeDashEnded + dashCooldown;
    //&& (grounded || lastGroundedTime > timeDashEnded);
    public bool CanFly => flyTimeUsed < maxFlyDuration;

    // Events
    public void InvokeJumped() => Jumped?.Invoke();
    public void InvokeDashed() => Dashed?.Invoke();
    public void InvokeDashEnded() => DashEnded?.Invoke();
    public void InvokeGroundedChanged(bool x, float y) => GroundedChanged?.Invoke(x, y);
    public void InvokeFlyStarted() => FlyStarted?.Invoke();
    public void InvokeFlyEnded() => FlyEnded?.Invoke();
    public void InvokeGroundSlamStarted() => GroundSlamStarted?.Invoke();
    public void InvokeGroundSlamEnded() => GroundSlamEnded?.Invoke();
}