using UnityEngine;

public class PlayerFly : State
{
    readonly PlayerContext ctx;

    public PlayerFly(StateMachine m, State parent, PlayerContext ctx) : base(m, parent)
    {
        this.ctx = ctx;
    }

    protected override State GetTransition()
    {
        if (!ctx.jumpHeld || !ctx.CanFly)
        {
            return ((PlayerRoot)Parent).PlayerAirborne.PlayerFall;
        }

        return null;
    }

    protected override void OnUpdate(float deltaTime)
    {
        ctx.flyTimeUsed += deltaTime;
    }

    protected override void OnFixedUpdate(float fixedDeltaTime)
    {
        ctx.frameVelocity.x = MovementUtils.ApplyHorizontal(ctx.frameVelocity.x, ctx.inputVec.x, ctx.maxHorizontalSpeed, ctx.acceleration, ctx.flyDeceleration, fixedDeltaTime);
        ctx.frameVelocity.y = ctx.flySpeed;
    }

    protected override void OnEnter()
    {
        ctx.frameVelocity.y = ctx.flySpeed;
        ctx.InvokeFlyStarted();
    }

    protected override void OnExit()
    {
        ctx.frameVelocity.y = 0f;
        ctx.InvokeFlyEnded();
    }
}
