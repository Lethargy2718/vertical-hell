using UnityEngine;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
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

    private float _displayedHealth;
    private float _targetHealth;
    private Coroutine _rollRoutine;

    private void OnEnable() => playerHealth.OnHealthChanged += UpdateHealthUI;
    private void OnDisable() => playerHealth.OnHealthChanged -= UpdateHealthUI;

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
                t += Time.deltaTime / scaledDuration;
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

    private void UpdateText(float value)
    {
        healthText.text = $"{Mathf.Ceil(value)} / {playerHealth.MaxHealth}";
    }
}