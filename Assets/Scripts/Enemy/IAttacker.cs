using System;
using UnityEngine;

public interface IAttacker
{
    public void StartAttacking();
    public void StopAttacking();
    public void SetAttackSpeedMultiplier(float multiplier);
    public Transform Target { get; set; }
    public event Action<float> ChargeUpStarted;
    public event Action<float> ChargeDownStarted;
    public event Action<float> CooldownStarted;
}