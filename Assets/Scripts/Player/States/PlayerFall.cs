using UnityEngine;

public class PlayerFall : State
{
    readonly PlayerContext ctx;

    public PlayerFall(StateMachine m, State parent, PlayerContext ctx) : base(m, parent)
    {
        this.ctx = ctx;
    }

    protected override State GetTransition()
    {
        if (ctx.grounded)
        {
            return ((PlayerRoot)Parent.Parent).PlayerGrounded;
        }

        if (ctx.jumpPressed)
        {
            if (ctx.inputVec.y < 0)
            {
                return ((PlayerAirborne)Parent).PlayerGroundSlam;
            }
        }

        return null;
    }

    protected override void OnFixedUpdate(float fixedDeltaTime)
    {
        HandleGravity(fixedDeltaTime);
    }

    private void HandleGravity(float dt)
    {
        float gravityThisFrame = ctx.fallAcceleration * dt;

        // TODO: do if ended fly quickly
        //if (ctx.endedJumpEarly && ctx.frameVelocity.y > 0)
        //{
        //    gravityThisFrame *= ctx.jumpEndEarlyGravityModifier;
        //}

        ctx.frameVelocity.y += -gravityThisFrame;
        ctx.frameVelocity.y = Mathf.Max(ctx.frameVelocity.y, -ctx.maxFallSpeed);
    }
}
