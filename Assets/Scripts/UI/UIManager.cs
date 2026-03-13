using UnityEngine;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private HealthComponent playerHealth;
    [SerializeField] private TextMeshProUGUI healthText;

    [Header("Health Roll")]
    [SerializeField] private float duration = 1.5f;
    [SerializeField] private float minDuration = 0.3f;

    [SerializeField]
    private AnimationCurve rollCurve = new AnimationCurve(
        new Keyframe(0, 0, 0, 3),
        new Keyframe(0.5f, 0.8f),
        new Keyframe(1, 1, 0, 0)
    );

    private Coroutine _rollRoutine;
    private float _displayedHealth;
    private float _targetHealth;

    [Header("Death Dim")]
    [SerializeField] private float dimDuration = 1.2f;
    [SerializeField]
    private AnimationCurve dimCurve = new AnimationCurve(
        new Keyframe(0, 1, 0, 0),
        new Keyframe(1, 0, -2, 0)
    );
    private const string _FACE_COLOR = "_FaceColor";

    private Color FaceColor
    {
        get => healthText.fontMaterial.GetColor(_FACE_COLOR);
        set => healthText.fontMaterial.SetColor(_FACE_COLOR, value);
    }
    private void Awake()
    {
        healthText.fontMaterial = new Material(healthText.fontMaterial);
    }

    private void OnEnable() => playerHealth.HealthChanged += UpdateHealthUI;
    private void OnDisable() => playerHealth.HealthChanged -= UpdateHealthUI;

    private void Start()
    {
        _displayedHealth = playerHealth.Health;
        _targetHealth = playerHealth.Health;
        UpdateText(_displayedHealth);
    }

    private void UpdateHealthUI(float currentHealth)
    {
        _targetHealth = currentHealth;
        _rollRoutine ??= StartCoroutine(RollCoroutine());
    }

    private IEnumerator RollCoroutine()
    {
        while (!Mathf.Approximately(_displayedHealth, _targetHealth))
        {
            float startHealth = _displayedHealth;
            float lockedTarget = _targetHealth;
            float change = Mathf.Abs(lockedTarget - startHealth);
            float scaledDuration = Mathf.Max(duration * (change / playerHealth.MaxHealth), minDuration);
            float t = 0f;

            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / scaledDuration;
                t = Mathf.Clamp01(t);
                _displayedHealth = Mathf.Lerp(startHealth, lockedTarget, rollCurve.Evaluate(t));
                UpdateText(_displayedHealth);

                if (!Mathf.Approximately(_targetHealth, lockedTarget))
                    break;

                yield return null;
            }

            yield return null;
        }

        _displayedHealth = _targetHealth;
        UpdateText(_displayedHealth);
        _rollRoutine = null;
    }

    public void DimUI(float dimDuration)
    {
        StartCoroutine(DimCoroutine(dimDuration));
    }

    private IEnumerator DimCoroutine(float dimDuration)
    {
        var (startColor, startIntensity) = FaceColor.Decompose();

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / dimDuration;
            t = Mathf.Clamp01(t);

            float intensity = dimCurve.Evaluate(t) * startIntensity;
            FaceColor = startColor.WithIntensity(intensity);
            yield return null;
        }

        // Make sure it's reset if the curve was erroneous
        FaceColor = FaceColor.WithIntensity(0f);
    }

    private void UpdateText(float value)
    {
        healthText.text = $"{Mathf.Ceil(value)} / {playerHealth.MaxHealth}";
    }
}

