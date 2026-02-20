using UnityEngine;

public class LowHealthEffects : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private HealthComponent healthComponent;

    [Header("Post Processing")]
    [SerializeField] private Color vignetteColor = Color.red;
    [SerializeField] private float maxVignetteBoost = 0.4f;
    [SerializeField] private float maxChromaticBoost = 0.6f;
    [SerializeField] private float threshold = 0.5f;

    private readonly PostProcessModifier _modifier = new PostProcessModifier();

    private void Start()
    {
        _modifier.vignetteColor = vignetteColor;
        PostProcessController.Instance.AddModifier(_modifier);
    }

    private void OnEnable() => healthComponent.HealthChanged += HandleHealthChanged;
    private void OnDisable() => healthComponent.HealthChanged -= HandleHealthChanged;

    private void HandleHealthChanged(float current)
    {
        float healthPercent = current / healthComponent.MaxHealth;

        float intensity = Mathf.Clamp01(1f - (healthPercent / threshold));

        _modifier.vignetteOffset = maxVignetteBoost * intensity;
        _modifier.chromaticOffset = maxChromaticBoost * intensity;
    }
}