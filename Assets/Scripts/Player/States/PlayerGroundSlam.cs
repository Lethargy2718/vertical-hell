using UnityEngine;

public class PlayerGroundSlam : State
{
    readonly PlayerContext ctx;

    public PlayerGroundSlam(StateMachine m, State parent, PlayerContext ctx) : base(m, parent)
    {
        this.ctx = ctx;
    }

    protected override State GetTransition()
    {
        if (ctx.grounded)
        {
            return ((PlayerRoot)Parent.Parent).PlayerGrounded;
        }
        return null;
    }

    protected override void OnFixedUpdate(float fixedDeltaTime)
    {
        ctx.frameVelocity.y = -ctx.groundSlamSpeed;
    }

    protected override void OnEnter()
    {
        ctx.frameVelocity.y = -ctx.groundSlamSpeed;
        ctx.InvokeGroundSlamStarted();
    }

    protected override void OnExit()
    {
        ctx.frameVelocity.y = 0f;
        ctx.InvokeGroundSlamEnded();
    }
}
