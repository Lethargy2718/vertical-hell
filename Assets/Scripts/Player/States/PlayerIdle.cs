using UnityEngine;

public class PlayerIdle : State
{
    readonly PlayerContext ctx;

    public PlayerIdle(StateMachine m, State parent, PlayerContext ctx) : base(m, parent)
    {
        this.ctx = ctx;
    }

    protected override State GetTransition()
    {
        if (Mathf.Abs(ctx.inputVec.x) > 0.01f) {
            return ((PlayerGrounded)Parent).PlayerMove;
        }
        return null;
    }

    protected override void OnEnter()
    {
        // Leave y = groundingForce
        ctx.frameVelocity.x = 0f;
    }
}
