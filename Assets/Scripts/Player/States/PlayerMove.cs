using UnityEngine;

public class PlayerMove : State
{
    readonly PlayerContext ctx;

    public PlayerMove(StateMachine m, State parent, PlayerContext ctx) : base(m, parent)
    {
        this.ctx = ctx;
    }

    protected override State GetTransition()
    {
        if (Mathf.Abs(ctx.inputVec.x) <= 0.01f)
        {
            return ((PlayerGrounded)Parent).PlayerIdle;
        }

        return null;
    }

    protected override void OnFixedUpdate(float fixedDeltaTime)
    {
        ctx.frameVelocity.x = MovementUtils.ApplyHorizontal(ctx.frameVelocity.x, ctx.inputVec.x, ctx.maxHorizontalSpeed, ctx.acceleration, ctx.groundDeceleration, fixedDeltaTime);
    }
}

