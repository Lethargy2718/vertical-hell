using System;
using System.Linq;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class PlayerStateDriver : MonoBehaviour
{
    public PlayerContext ctx = new PlayerContext();

    // TODO: move into context if the HSM ever handles collisions
    [Header("Collision")]
    [SerializeField] private float collisionCheckDistance = 0.05f;
    [SerializeField] private LayerMask excludeFromCollisions;

    [SerializeField] public bool drawGizmos = true;
    [SerializeField] private TextMeshProUGUI stateText;

    string lastPath;
    StateMachine machine;
    private bool groundHit;
    private bool ceilingHit;

    void Awake()
    {
        ctx.rb = GetComponent<Rigidbody2D>();
        ctx.col = GetComponent<CapsuleCollider2D>();
        ctx.sr = GetComponentInChildren<SpriteRenderer>();

        ctx.rb.freezeRotation = true;
        ctx.rb.gravityScale = 0f;

        ctx.globalQueryStartInColliders = Physics2D.queriesStartInColliders;

        var root = new PlayerRoot(null, ctx);
        var builder = new StateMachineBuilder(root);
        machine = builder.Build();
    }

    void Update()
    {
        ctx.time += Time.deltaTime;
        GatherInput();
        HandleSpriteFlip();

        machine.Tick(Time.deltaTime);
    }

    void FixedUpdate()
    {
        CheckCollisions();
        ctx.rb.linearVelocity = ctx.frameVelocity;
        machine.FixedTick(Time.fixedDeltaTime);
    }

    private void LateUpdate()
    {
        var path = StatePath(machine.Root.Leaf());
        if (path != lastPath)
        {
            lastPath = path;
            stateText.text = path;
        }
    }

    // TODO: use new input system
    void GatherInput()
    {
        // Jump
        ctx.jumpPressed = Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space);
        ctx.jumpHeld = Input.GetButton("Jump") || Input.GetKey(KeyCode.Space);

        if (ctx.jumpPressed)
        {
            ctx.timeJumpWasPressed = ctx.time;
            ctx.hasBufferedJump = true;
        }

        // Dash
        ctx.dashPressed = Input.GetKeyDown(KeyCode.E)
                    || Input.GetKeyDown(KeyCode.LeftControl)
                    || Input.GetKeyDown(KeyCode.Mouse3);

        if (ctx.dashPressed)
        {
            ctx.timeDashWasPressed = ctx.time;
            ctx.hasBufferedDash = true;
        }

        // Horizontal
        bool lp = Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow);
        bool rp = Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow);
        bool lh = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
        bool rh = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);
        ctx.inputVec.x = HandleInputPriority(lp, rp, lh, rh, ctx.inputVec.x);
        if (ctx.inputVec.x != 0f) ctx.currentHorizontalDirection = ctx.inputVec.x;

        // Vertical
        bool dp = Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow);
        bool up = Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);
        bool dh = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
        bool uh = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
        ctx.inputVec.y = HandleInputPriority(dp, up, dh, uh, ctx.inputVec.y);
        if (ctx.inputVec.y != 0f) ctx.currentVerticalDirection = ctx.inputVec.y;
    }

    static float HandleInputPriority(bool pressed1, bool pressed2, bool held1, bool held2, float oldVal)
    {
        if (pressed1 && pressed2) return 1f;
        if (pressed1) return -1f;
        if (pressed2) return 1f;
        if (held1 && held2) return oldVal;
        if (held1) return -1f;
        if (held2) return 1f;
        return 0f;
    }

    // TODO: possibly move to root?
    void CheckCollisions()
    {
        Physics2D.queriesStartInColliders = false;

        groundHit = Physics2D.CapsuleCast(ctx.col.bounds.center, ctx.col.size, ctx.col.direction, 0, Vector2.down, collisionCheckDistance, ~excludeFromCollisions);
        ceilingHit = Physics2D.CapsuleCast(ctx.col.bounds.center, ctx.col.size, ctx.col.direction, 0, Vector2.up, collisionCheckDistance, ~excludeFromCollisions);

        if (ceilingHit)
        {
            ctx.frameVelocity.y = Mathf.Min(0, ctx.frameVelocity.y);
        }

        ctx.grounded = groundHit;

        Physics2D.queriesStartInColliders = ctx.globalQueryStartInColliders;
    }

    // TODO: handle on a component
    void HandleSpriteFlip()
    {
        if (ctx.inputVec.x > 0f) ctx.sr.flipX = false;
        else if (ctx.inputVec.x < 0f) ctx.sr.flipX = true;
    }

    static string StatePath(State s) =>
        string.Join("\n > ", s.PathToRoot().Reverse().Select(n => n.GetType().Name));


    // TODO: move this stuff to a helper
    private void OnDrawGizmos()
    {
        if (!drawGizmos || ctx.col == null) return;

        Vector2 center = ctx.col.bounds.center;
        Vector2 size = ctx.col.size;

        // Ground
        Gizmos.color = groundHit ? Color.green : Color.red;
        DrawWireCapsule2D(center, size, ctx.col.direction);

        Gizmos.color = Color.yellow;
        DrawWireCapsule2D(center + Vector2.down * collisionCheckDistance, size, ctx.col.direction);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(center, center + Vector2.down * collisionCheckDistance);

        // Ceiling
        Gizmos.color = ceilingHit ? Color.green : Color.red;
        DrawWireCapsule2D(center, size, ctx.col.direction); // same origin, drawn on top

        Gizmos.color = new Color(1f, 0.5f, 0f); // orange
        DrawWireCapsule2D(center + Vector2.up * collisionCheckDistance, size, ctx.col.direction);

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(center, center + Vector2.up * collisionCheckDistance);
    }

    private void DrawArc(Vector2 center, float radius, float startAngle, float endAngle, int segments = 20)
    {
        float step = (endAngle - startAngle) / segments;
        Vector2 prev = center + new Vector2(
            Mathf.Cos(startAngle * Mathf.Deg2Rad),
            Mathf.Sin(startAngle * Mathf.Deg2Rad)) * radius;

        for (int i = 1; i <= segments; i++)
        {
            float angle = (startAngle + step * i) * Mathf.Deg2Rad;
            Vector2 next = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }

    private void DrawWireCapsule2D(Vector2 center, Vector2 size, CapsuleDirection2D direction)
    {
        float w = size.x / 2f;
        float h = size.y / 2f;

        if (direction == CapsuleDirection2D.Vertical)
        {
            float radius = w;
            float bodyHalf = h - radius;

            // Two semicircles at top and bottom
            DrawArc(center + Vector2.up * bodyHalf, radius, 0f, 180f);
            DrawArc(center + Vector2.down * bodyHalf, radius, 180f, 360f);

            // Side lines
            Gizmos.DrawLine(center + new Vector2(-radius, -bodyHalf), center + new Vector2(-radius, bodyHalf));
            Gizmos.DrawLine(center + new Vector2(radius, -bodyHalf), center + new Vector2(radius, bodyHalf));
        }
        else
        {
            float radius = h;
            float bodyHalf = w - radius;

            DrawArc(center + Vector2.right * bodyHalf, radius, -90f, 90f);
            DrawArc(center + Vector2.left * bodyHalf, radius, 90f, 270f);

            Gizmos.DrawLine(center + new Vector2(-bodyHalf, -radius), center + new Vector2(bodyHalf, -radius));
            Gizmos.DrawLine(center + new Vector2(-bodyHalf, radius), center + new Vector2(bodyHalf, radius));
        }
    }
}
