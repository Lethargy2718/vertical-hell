using System.Collections;
using UnityEngine;

public class PlayerDash : State
{
    readonly PlayerContext ctx;
    float timer;
    bool endDash = false;

    public PlayerDash(StateMachine m, State parent, PlayerContext ctx) : base(m, parent)
    {
        this.ctx = ctx;
    }

    protected override State GetTransition()
    {
        if (endDash)
        {
            if (ctx.grounded)
            {
                return ((PlayerRoot)Parent).PlayerGrounded;
            }

            return ((PlayerRoot)Parent).PlayerAirborne;
        }

        if (ctx.jumpPressed)
        {
            if (ctx.grounded)
            {
                return ((PlayerRoot)Parent).PlayerAirborne.PlayerJump;
            }

            return ((PlayerRoot)Parent).PlayerFly;
        }

        return null;
    }

    protected override void OnEnter()
    {
        endDash = false;
        timer = 0f;

        Vector2 dashDirection = GetDashDirection();
        ctx.frameVelocity = ctx.dashSpeed * dashDirection;

        ctx.hasBufferedDash = false;
        ctx.InvokeDashed();
    }

    private Vector2 GetDashDirection()
    {
        if (ctx.inputVec.x == 0f && ctx.inputVec.y != 0f)
        {
            return ctx.inputVec;
        }
        return new Vector2(ctx.currentHorizontalDirection, ctx.inputVec.y).normalized;
    }

    protected override void OnUpdate(float deltaTime)
    {
        timer += deltaTime;
        if (timer >= ctx.dashDuration)
        {
            endDash = true;
        }
    }

    protected override void OnExit()
    {
        ctx.timeDashEnded = ctx.time;
        //ctx.frameVelocity = new Vector2(0f, ctx.frameVelocity.y);
        //ctx.frameVelocity = Vector2.zero;
        ctx.InvokeDashEnded();
    }
}
