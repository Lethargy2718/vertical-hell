using System;
using UnityEngine;

public class HealthComponent : MonoBehaviour
{
    public Action<float> OnHealthChanged;
    public Action OnHealthDepleted;

    [SerializeField] private float _health = 100f;
    public float Health {
        get => _health;
        set
        {
            _health = Mathf.Clamp(value, 0, MaxHealth);
            OnHealthChanged?.Invoke(_health);

            if (_health == 0)
            {
                OnHealthDepleted?.Invoke();
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
}
