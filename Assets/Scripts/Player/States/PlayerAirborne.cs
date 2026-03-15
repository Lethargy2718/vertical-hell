using UnityEngine;

public class PlayerAirborne : State
{
    readonly PlayerContext ctx;
    public readonly PlayerFall PlayerFall;
    public readonly PlayerFly PlayerFly;
    public readonly PlayerGroundSlam PlayerGroundSlam;

    public PlayerAirborne(StateMachine m, State parent, PlayerContext ctx) : base(m, parent)
    {
        this.ctx = ctx;
        PlayerFall = new PlayerFall(m, this, ctx);
        PlayerFly = new PlayerFly(m, this, ctx);
        PlayerGroundSlam = new PlayerGroundSlam(m, this, ctx);
    }

    protected override State GetInitialState() => PlayerFall;

    protected override void OnFixedUpdate(float fixedDeltaTime)
    {
        ctx.frameVelocity.x = MovementUtils.ApplyHorizontal(ctx.frameVelocity.x, ctx.inputVec.x, ctx.maxHorizontalSpeed, ctx.acceleration, ctx.airDeceleration, fixedDeltaTime);
    }


}
