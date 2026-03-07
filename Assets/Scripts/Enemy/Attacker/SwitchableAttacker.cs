using System;
using System.Linq;
using UnityEngine;

public class SwitchableAttacker : MonoBehaviour, IAttacker
{
    public event Action<float> ChargeUpStarted;
    public event Action<float> ChargeDownStarted;
    public event Action<float> CooldownStarted;

    private IAttacker[] strategies;
    private IAttacker current;
    private bool isAttacking;

    public Transform Target { get; set; }

    private void Awake()
    {
        strategies = GetComponents<IAttacker>()
            .Where(a => a != (IAttacker)this)
            .ToArray();

        foreach (var s in strategies)
            SetStrategyEnabled(s, false);
    }

    private void Start()
    {
        if (strategies.Length > 0)
            SwitchTo(strategies[0]);

        foreach (IAttacker attacker in strategies)
            attacker.Target = Target;
    }

    public void SwitchTo(IAttacker strategy)
    {
        if (current != null)
        {
            current.StopAttacking();
            UnsubscribeFrom(current);
            SetStrategyEnabled(current, false);
        }

        current = strategy;
        SetStrategyEnabled(current, true);
        SubscribeTo(current);

        if (isAttacking)
            current.StartAttacking();
    }

    public void SwitchTo<T>() where T : IAttacker
    {
        IAttacker strategy = Array.Find(strategies, s => s is T);
        SwitchTo(strategy);
    }

    public void StartAttacking()
    {
        isAttacking = true;
        current?.StartAttacking();
    }

    public void StopAttacking()
    {
        isAttacking = false;
        current?.StopAttacking();
    }

    public void SetAttackSpeedMultiplier(float multiplier) => current?.SetAttackSpeedMultiplier(multiplier);

    private void SubscribeTo(IAttacker attacker)
    {
        attacker.ChargeUpStarted += OnChargeUpStarted;
        attacker.ChargeDownStarted += OnChargeDownStarted;
        attacker.CooldownStarted += OnCooldownStarted;
    }

    private void UnsubscribeFrom(IAttacker attacker)
    {
        attacker.ChargeUpStarted -= OnChargeUpStarted;
        attacker.ChargeDownStarted -= OnChargeDownStarted;
        attacker.CooldownStarted -= OnCooldownStarted;
    }

    private void SetStrategyEnabled(IAttacker attacker, bool enabled)
    {
        if (attacker is Behaviour b)
            b.enabled = enabled;
    }

    private void OnChargeUpStarted(float d) => ChargeUpStarted?.Invoke(d);
    private void OnChargeDownStarted(float d) => ChargeDownStarted?.Invoke(d);
    private void OnCooldownStarted(float d) => CooldownStarted?.Invoke(d);
}