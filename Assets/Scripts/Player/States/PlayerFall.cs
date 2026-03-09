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

        if (ctx.CanUseBufferedJump && ctx.CanUseCoyote)
        {
            return ((PlayerAirborne)Parent).PlayerJump;
        }

        if (ctx.jumpPressed)
        {
            if (ctx.inputVec.y < 0)
            {
                return ((PlayerRoot)Parent.Parent).PlayerGroundSlam;
            }
            
            if (ctx.CanFly)
            {
                return ((PlayerRoot)Parent.Parent).PlayerFly;

            }
        }

        return null;
    }
}
