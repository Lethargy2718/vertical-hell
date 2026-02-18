using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private HealthComponent playerHealth;
    [SerializeField] private TextMeshProUGUI healthText;

    private void OnEnable()
    {
        playerHealth.OnHealthChanged += UpdateHealthUI;
    }

    private void OnDisable()
    {
        playerHealth.OnHealthChanged -= UpdateHealthUI;
    }

    private void Start()
    {
        // Initialize UI
        UpdateHealthUI(playerHealth.Health);
    }

    private void UpdateHealthUI(float currentHealth)
    {
        healthText.text = $"{currentHealth} / {playerHealth.MaxHealth}";
    }
}
