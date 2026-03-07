using System.Diagnostics;

public class PlayerRoot : State
{
    readonly PlayerContext ctx;
    public readonly PlayerGrounded PlayerGrounded;
    public readonly PlayerAirborne PlayerAirborne;
    public readonly PlayerFly PlayerFly;
    public readonly PlayerDash PlayerDash;

    public PlayerRoot(StateMachine m, PlayerContext ctx) : base(m, null)
    {
        this.ctx = ctx;
        PlayerGrounded = new PlayerGrounded(m, this, ctx);
        PlayerAirborne = new PlayerAirborne(m, this, ctx);
        PlayerFly = new PlayerFly(m, this, ctx);
        PlayerDash = new PlayerDash(m, this, ctx);
    }

    protected override State GetInitialState() => PlayerGrounded;

    protected override State GetTransition()
    {
        if (ctx.CanUseBufferedDash && Leaf() != PlayerDash)
        {
            return PlayerDash;
        }

        return null;
    }
}
