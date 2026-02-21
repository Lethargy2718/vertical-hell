using System;

public interface IAttacker
{
    public void StartAttacking();
    public void StopAttacking();
    public event Action<float> ChargeUpStarted;
    public event Action<float> ChargeDownStarted;
    public event Action<float> CooldownStarted;
}
