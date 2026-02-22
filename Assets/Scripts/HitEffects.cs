using System.Collections;
using Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;

public class HitEffects : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private float effectDuration = 0.5f;

    [Header("Damage Notifier")]
    [SerializeField] private HealthComponent healthComponent;

    [Header("Time Slow Parameters")]
    [SerializeField] private bool enableSlowMo = false;
    [SerializeField] private float slowMoTimeScale = 0.1f;

    [Header("Camera Pulse Parameters")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private float pulseAmount = 1f;
    [SerializeField] private AnimationCurve pulseCurve = new AnimationCurve(
        new Keyframe(0, 0),
        new Keyframe(0.3f, 1f),
        new Keyframe(1, 0)
    );
    private Coroutine _effectRoutine;

    [Header("Screen Shake Parameters")]
    [SerializeField] private float shakeAmplitude = 0.14f; // When not using slowmo
    [SerializeField] private float shakeFrequency = 25f;
    private CinemachineBasicMultiChannelPerlin _noise;


    [Header("Post Processing")]
    [SerializeField] private Volume postProcessVolume;
    [SerializeField] private Color vignetteColor = Color.red;
    [SerializeField] private float vignetteIntensityBoost = 0.3f;
    [SerializeField] private float chromaticAberrationAmount = 1f;
    private readonly PostProcessModifier _postProcessModifier = new PostProcessModifier();

    private SoundComponent _sc;

    private void Awake()
    {
        _sc = GetComponent<SoundComponent>();
    }

    private void Start()
    {
        _noise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        _noise.m_AmplitudeGain = 0f;
        _noise.m_FrequencyGain = shakeFrequency;

        _postProcessModifier.vignetteColor = vignetteColor;
        PostProcessController.Instance.AddModifier(_postProcessModifier);
    }

    private void OnEnable() => healthComponent.DamageTaken += HandleHit;
    private void OnDisable() => healthComponent.DamageTaken -= HandleHit;

    private void HandleHit(float damage, Vector2 _)
    {
        _sc.Play();
        if (_effectRoutine != null)
            StopCoroutine(_effectRoutine);
        _effectRoutine = StartCoroutine(HitRoutine());
    }

    private IEnumerator HitRoutine()
    {
        if (enableSlowMo) EnterSlowMo();
        yield return StartCoroutine(AnimateEffects(effectDuration));
        if (enableSlowMo) ExitSlowMo();
        _effectRoutine = null;
    }

    private IEnumerator AnimateEffects(float duration)
    {
        float elapsed = 0f;

        float startSize = virtualCamera.m_Lens.OrthographicSize;
        float zoomedSize = startSize + pulseAmount;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float curveValue = pulseCurve.Evaluate(t);

            // Lens zoom
            var lens = virtualCamera.m_Lens;
            lens.OrthographicSize = Mathf.LerpUnclamped(startSize, zoomedSize, curveValue);
            virtualCamera.m_Lens = lens;

            // Screen shake
            _noise.m_PivotOffset = Random.insideUnitSphere * 100f;                          // big enough to always land in a visually distinct region in the perlin field
            _noise.m_AmplitudeGain = Mathf.LerpUnclamped(0f, shakeAmplitude, curveValue);   // unclamped to allow overshooting

            // Vignette
            _postProcessModifier.vignetteOffset = vignetteIntensityBoost * curveValue;

            // Chromatic aberration
            _postProcessModifier.chromaticOffset = chromaticAberrationAmount * curveValue;

            yield return null;
        }
    }

    // Better avoid this for multiplayer

    #region SlowMo

    private void EnterSlowMo()
    {
        Time.timeScale = slowMoTimeScale;
        Time.fixedDeltaTime *= slowMoTimeScale;
    }

    private void ExitSlowMo()
    {
        Time.fixedDeltaTime /= slowMoTimeScale;
        Time.timeScale = 1f;
    }

    #endregion
}