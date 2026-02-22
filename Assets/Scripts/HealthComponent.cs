using System;
using System.Collections;
using UnityEngine;

public class HealthComponent : MonoBehaviour
{
    public enum DamageType { Normal, Effect, Absolute };
    public Action<float> HealthChanged;
    public Action<float> DamageTaken;
    public Action HealthDepleted;
    public Action InvincibilityStarted;
    public Action InvincibilityEnded;

    private int _invincibleEffects = 0;
    public void AddInvincibleEffect()
    {
        Debug.Log(_invincibleEffects + " is getting increased");

        _invincibleEffects++;
        if (_invincibleEffects == 1)
        {
            InvincibilityStarted?.Invoke();
        }
    }

    public void RemoveInvincibleEffect()
    {
        Debug.Log(_invincibleEffects + " is getting reduced");

        if (_invincibleEffects == 1)
        {
            InvincibilityEnded?.Invoke();
        }

        _invincibleEffects = Mathf.Max(0, _invincibleEffects - 1);
    }

    public bool IsInvincible => _invincibleEffects > 0;

    [SerializeField] private float _health = 100f;
    public float Health {
        get => _health;
        set
        {
            _health = Mathf.Clamp(value, 0, MaxHealth);
            HealthChanged?.Invoke(_health);

            if (_health == 0)
            {
                HealthDepleted?.Invoke();
            }
        }
    }

    [SerializeField] private float _maxHealth = 100f;
    public float MaxHealth
    {
        get => _maxHealth;
        set => _maxHealth = Mathf.Max(0, value);
    }

    [SerializeField] private float invincibilityDuration = 1.0f;

    private void Start()
    {
        Health = MaxHealth;
    }

    public void TakeDamage(float dmg, DamageType damageType = DamageType.Normal)
    {
        if (IsInvincible && damageType == DamageType.Normal) return;

        Health -= dmg;

        if (damageType == DamageType.Normal)
        {
            DamageTaken?.Invoke(Health);
            StartCoroutine(InvincibilityCoroutine());
        }
    }

    private IEnumerator InvincibilityCoroutine()
    {
        AddInvincibleEffect();
        yield return new WaitForSeconds(invincibilityDuration);
        RemoveInvincibleEffect();
    }
}
