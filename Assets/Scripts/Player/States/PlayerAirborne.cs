using UnityEngine;

public class PlayerAirborne : State
{
    readonly PlayerContext ctx;
    public readonly PlayerJump PlayerJump;
    public readonly PlayerFall PlayerFall;

    public PlayerAirborne(StateMachine m, State parent, PlayerContext ctx) : base(m, parent)
    {
        this.ctx = ctx;
        PlayerJump = new PlayerJump(m, this, ctx);
        PlayerFall = new PlayerFall(m, this, ctx);
    }

    protected override State GetInitialState() => PlayerFall;

    protected override void OnFixedUpdate(float fixedDeltaTime)
    {
        HandleGravity(fixedDeltaTime);
        ctx.frameVelocity.x = MovementUtils.ApplyHorizontal(ctx.frameVelocity.x, ctx.inputVec.x, ctx.maxHorizontalSpeed, ctx.acceleration, ctx.airDeceleration, fixedDeltaTime);
    }

    private void HandleGravity(float dt)
    {
        //if (ctx.grounded && ctx.frameVelocity.y <= 0)
        //{
        //    ctx.frameVelocity.y = ctx.groundingForce;
        //    return;
        //}
        float gravityThisFrame = ctx.fallAcceleration * dt;

        if (ctx.endedJumpEarly && ctx.frameVelocity.y > 0)
        {
            gravityThisFrame *= ctx.jumpEndEarlyGravityModifier;
        }

        ctx.frameVelocity.y += -gravityThisFrame;
        ctx.frameVelocity.y = Mathf.Max(ctx.frameVelocity.y, -ctx.maxFallSpeed);
    }
}
