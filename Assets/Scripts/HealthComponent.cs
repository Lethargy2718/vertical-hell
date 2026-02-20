using System;
using Cinemachine;
using UnityEngine;

public class HealthComponent : MonoBehaviour
{
    public Action<float> HealthChanged;
    public Action<float> DamageTaken;
    public Action HealthDepleted;

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

    private void Start()
    {
        Health = MaxHealth;
    }

    public void TakeDamage(float dmg)
    {
        Health -= dmg;
        DamageTaken?.Invoke(Health);
    }
}
