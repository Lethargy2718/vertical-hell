using UnityEngine;
using TMPro;
using System.Collections;

public class LowHealthEffects : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private HealthComponent healthComponent;

    [Header("Text Shake")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private float maxCharShakeAmount = 2f;

    [Header("Post Processing")]
    [SerializeField] private Color vignetteColor = Color.red;
    [SerializeField] private float maxVignetteBoost = 0.4f;
    [SerializeField] private float maxChromaticBoost = 0.6f;
    [SerializeField] private float threshold = 0.5f;

    private readonly PostProcessModifier _modifier = new PostProcessModifier();
    private float _currentIntensity;
    private Coroutine _shakeRoutine;

    private void Start()
    {
        _modifier.vignetteColor = vignetteColor;
        PostProcessController.Instance.AddModifier(_modifier);
    }

    private void Update()
    {
        if (_currentIntensity <= 0f) return;
    }

    private void OnEnable() => healthComponent.HealthChanged += HandleHealthChanged;
    private void OnDisable() => healthComponent.HealthChanged -= HandleHealthChanged;

    private void HandleHealthChanged(float current)
    {
        float healthPercent = current / healthComponent.MaxHealth;

        // Clamp keeps intensity at 0 until healthPercent is less than threshold
        _currentIntensity = Mathf.Clamp01(1f - (healthPercent / threshold)); 

        _modifier.vignetteOffset = maxVignetteBoost * _currentIntensity;
        _modifier.chromaticOffset = maxChromaticBoost * _currentIntensity;

        if (_currentIntensity > 0f)
            StartShaking();
        else
            StopShaking();
    }
    
    public void StartShaking()
    {
        if (_shakeRoutine != null) return; // already running
        _shakeRoutine = StartCoroutine(ShakeRoutine());
    }

    public void StopShaking()
    {
        if (_shakeRoutine == null) return;
        StopCoroutine(_shakeRoutine);
        _shakeRoutine = null;
    }

    private IEnumerator ShakeRoutine()
    {
        while (true)
        {
            ShakeTextCharacters();
            yield return null;
        }
    }

    private void ShakeTextCharacters()
    {
        healthText.ForceMeshUpdate();
        TMP_TextInfo textInfo = healthText.textInfo;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue; // Skip whitespace

            int vertexIndex = charInfo.vertexIndex;
            int meshIndex = charInfo.materialReferenceIndex;
            Vector3[] vertices = textInfo.meshInfo[meshIndex].vertices;

            Vector3 offset = _currentIntensity * maxCharShakeAmount * Random.insideUnitCircle;

            vertices[vertexIndex + 0] += offset;
            vertices[vertexIndex + 1] += offset;
            vertices[vertexIndex + 2] += offset;
            vertices[vertexIndex + 3] += offset;
        }

        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
            healthText.UpdateGeometry(textInfo.meshInfo[i].mesh, i);    
        }
    }
}