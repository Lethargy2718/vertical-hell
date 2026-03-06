using System;

public interface IAttacker
{
    public void StartAttacking();
    public void StopAttacking();
    public void SetAttackSpeedMultiplier(float multiplier);
    public event Action<float> ChargeUpStarted;
    public event Action<float> ChargeDownStarted;
    public event Action<float> CooldownStarted;
}