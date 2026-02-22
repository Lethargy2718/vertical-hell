using UnityEngine;
using System.Collections;

public class InvincibilityEffects : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HealthComponent _health;
    private SpriteRenderer _spriteRenderer;

    [Header("Invincible Settings")]
    [SerializeField] private Color _invincibleColor = new Color(0f, 0f, 0f, 0f);
    [SerializeField] private float _flashSpeed = 10f;
    private Coroutine _invincibleEffect;
    private Color _originalColor;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _originalColor = _spriteRenderer.color;
    }

    private void OnEnable()
    {
        _health.InvincibilityStarted += OnInvincibilityStarted;
        _health.InvincibilityEnded += OnInvincibilityEnded;
    }

    private void OnDisable()
    {
        _health.InvincibilityStarted -= OnInvincibilityStarted;
        _health.InvincibilityEnded -= OnInvincibilityEnded;

        if (_invincibleEffect != null)
        {
            StopCoroutine(_invincibleEffect);
            _invincibleEffect = null;
        }

        ResetVisuals();
    }

    private void OnInvincibilityStarted()
    {
        if (_invincibleEffect != null)
            StopCoroutine(_invincibleEffect);

        _invincibleEffect = StartCoroutine(InvincibleEffect());
    }

    private void OnInvincibilityEnded()
    {
        Debug.Log("Ended" + gameObject);
        if (_invincibleEffect != null)
        {
            StopCoroutine(_invincibleEffect);
            _invincibleEffect = null;
        }

        ResetVisuals();
    }

    private IEnumerator InvincibleEffect()
    {
        while (true)
        {
            float t = (Mathf.Sin(Time.time * _flashSpeed) + 1) / 2;
            _spriteRenderer.color = Color.Lerp(_originalColor, _invincibleColor, t);

            yield return null;
        }
    }

    private void ResetVisuals()
    {
        _spriteRenderer.color = _originalColor;
    }
}