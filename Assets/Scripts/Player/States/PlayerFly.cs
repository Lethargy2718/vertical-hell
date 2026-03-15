using Cinemachine;
using DG.Tweening;
using UnityEngine;

public class PlayerFly : State
{
    readonly PlayerContext ctx;
    private Tween offsetTween;
    private CinemachineFramingTransposer transposer;
    private float offsetOrigin;

    public PlayerFly(StateMachine m, State parent, PlayerContext ctx) : base(m, parent)
    {
        this.ctx = ctx;
    }

    protected override State GetTransition()
    {
        if (!ctx.jumpHeld || !ctx.CanFly)
        {
            return ((PlayerAirborne)Parent).PlayerFall;
        }

        return null;
    }

    protected override void OnUpdate(float deltaTime)
    {
        ctx.flyTimeUsed += deltaTime;
    }

    protected override void OnFixedUpdate(float fixedDeltaTime)
    {
        ctx.frameVelocity.y = ctx.flySpeed;
    }

    protected override void OnEnter()
    {
        ctx.frameVelocity.y = ctx.flySpeed;
        ctx.InvokeFlyStarted();

        // I need a centralized manager like the postfx to make this work with the death handler panning

        //if (transposer == null)
        //{
        //    transposer = ctx.virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        //    offsetOrigin = transposer.m_TrackedObjectOffset.y;
        //}


        //offsetTween?.Kill();
        //offsetTween = TweenOffset(transposer, offsetOrigin + ctx.cameraOffset, ctx.cameraOffsetInDuration);
    }

    protected override void OnExit()
    {
        ctx.frameVelocity.y = 0f;
        ctx.InvokeFlyEnded();

        //offsetTween?.Kill();
        //offsetTween = TweenOffset(transposer, offsetOrigin, ctx.cameraOffsetOutDuration);
    }

    private Tween TweenOffset(CinemachineFramingTransposer transposer, float target, float duration) => DOTween.To(
        () => transposer.m_TrackedObjectOffset.y,
        y => transposer.m_TrackedObjectOffset = new Vector3(transposer.m_TrackedObjectOffset.x, y, 0),
        target,
        duration
    );
}