using UnityEngine;

public class LowHealthEffects : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private HealthComponent healthComponent;

    [Header("Post Processing")]
    [SerializeField] private float maxVignetteBoost = 0.4f;
    [SerializeField] private float maxChromaticBoost = 0.6f;
    [SerializeField] private float threshold = 0.5f;

    private readonly PostProcessModifier _modifier = new();

    private void Start() => PostProcessController.Instance.AddModifier(_modifier);
    private void OnEnable() => healthComponent.OnHealthChanged += HandleHealthChanged;
    private void OnDisable() => healthComponent.OnHealthChanged -= HandleHealthChanged;

    private void HandleHealthChanged(float current)
    {
        float healthPercent = current / healthComponent.MaxHealth;

        float intensity = Mathf.Clamp01(1f - (healthPercent / threshold));

        _modifier.vignetteOffset = maxVignetteBoost * intensity;
        _modifier.chromaticOffset = maxChromaticBoost * intensity;
    }
}