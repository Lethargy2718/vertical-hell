using UnityEngine;

public class PlayerJump : State
{
    readonly PlayerContext ctx;
    private float JumpForce => Mathf.Sqrt(2f * ctx.fallAcceleration * ctx.jumpHeight) + (ctx.fallAcceleration * UnityEngine.Time.fixedDeltaTime / 2f);

    public PlayerJump(StateMachine m, State parent, PlayerContext ctx) : base(m, parent)
    {
        this.ctx = ctx;
    }

    protected override void OnEnter()
    {
        ExecuteJump();
    }

    protected override State GetTransition() {
        if (!ctx.grounded && ctx.frameVelocity.y <= 0f) // Natural fall after apex
        {
            return ((PlayerAirborne)Parent).PlayerFall;
        }

        if (!ctx.jumpHeld && ctx.frameVelocity.y > 0) // Ended early => fast fall
        {
            ctx.endedJumpEarly = true;
            return ((PlayerAirborne)Parent).PlayerFall;
        }

        return null;
    }

    private void ExecuteJump()
    {
        ctx.endedJumpEarly = false;
        ctx.timeJumpWasPressed = float.MinValue;
        ctx.hasBufferedJump = false;
        ctx.coyoteUsable = false;
        ctx.frameVelocity.y = JumpForce;
        ctx.InvokeJumped();
    }

}
