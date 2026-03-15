using UnityEngine;

public class PlayerGrounded : State
{
    readonly PlayerContext ctx;
    public readonly PlayerIdle PlayerIdle;
    public readonly PlayerMove PlayerMove;

    public PlayerGrounded(StateMachine m, State parent, PlayerContext ctx) : base(m, parent)
    {
        this.ctx = ctx;
        PlayerIdle = new PlayerIdle(m, this, ctx);
        PlayerMove = new PlayerMove(m, this, ctx);
    }

    protected override State GetInitialState()
    {
        if (Mathf.Abs(ctx.frameVelocity.x) <= 0.01f)
        {
            return PlayerIdle;
        }
        return PlayerMove;
    }

    protected override State GetTransition()
    {
        return ctx.grounded ? null : ((PlayerRoot)Parent).PlayerAirborne;
    }

    protected override void OnEnter()
    {
        ctx.grounded = true;
        ctx.lastGroundedTime = ctx.time;
        ctx.frameVelocity.y = ctx.groundingForce;
        ctx.coyoteUsable = true;
        ctx.endedJumpEarly = false;
        ctx.InvokeGroundedChanged(true, Mathf.Abs(ctx.frameVelocity.y));
    }

    protected override void OnExit()
    {
        ctx.grounded = false;

        ctx.lastGroundedTime = ctx.time;
        ctx.flyTimeUsed = 0f;
        ctx.InvokeGroundedChanged(false, 0f);
    }

    // Might need if something breaks
    //protected override void OnFixedUpdate(float fixedDeltaTime)
    //{
    //    ctx.frameVelocity.y = ctx.groundingForce;
    //}
}
