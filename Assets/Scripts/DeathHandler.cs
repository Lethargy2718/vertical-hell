using Cinemachine;
using UnityEngine;
using System.Collections;

public class DeathHandler : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private float cameraDamping = 1f;
    [SerializeField] private float zoomAmount = 3f;
    [SerializeField] private float zoomDuration = 1.5f;
    [SerializeField] private float targetAmplitude = 1.5f;
    private CinemachineBasicMultiChannelPerlin noise;

    [Header("Post Processing")]
    [SerializeField] private Color vignetteColor = Color.red;
    [SerializeField] private float vignetteIntensity = 0.6f;
    [SerializeField] private float chromaticAmount = 1f;
    [SerializeField] private float ppDuration = 1.5f;
    private readonly PostProcessModifier _postProcessModifier = new();

    [Header("Player")]
    [SerializeField] private Transform player;
    private HealthComponent healthComponent;

    private void Awake()
    {
        healthComponent = player.GetComponent<HealthComponent>();
        noise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    private void Start()
    {
        _postProcessModifier.vignetteColor = vignetteColor;
        PostProcessController.Instance.AddModifier(_postProcessModifier);
    }

    private void OnEnable()
    {
        healthComponent.HealthDepleted += HandleDeath;
    }

    private void OnDisable()
    {
        healthComponent.HealthDepleted -= HandleDeath;
        PostProcessController.Instance.RemoveModifier(_postProcessModifier);
    }

    private void HandleDeath()
    {
        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        StopPlayer();
        StartCoroutine(RampUpNoise(targetAmplitude, zoomDuration));
        StartCoroutine(RampUpPostProcess(ppDuration));
        yield return StartCoroutine(PanCamera());
        StopNoise();
        yield return new WaitForSeconds(1f);
        DisintegratePlayer();
        // black/red screen, then restart button
    }

    private IEnumerator RampUpPostProcess(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            _postProcessModifier.vignetteOffset = Mathf.Lerp(0f, vignetteIntensity, t);
            _postProcessModifier.chromaticOffset = Mathf.Lerp(0f, chromaticAmount, t);
            yield return null;
        }
    }

    private void StopPlayer()
    {
        if (player.TryGetComponent<PlayerStateDriver>(out var playerStateDriver))
        {
            playerStateDriver.enabled = false;
        }

        if (player.TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.bodyType = RigidbodyType2D.Static;
        }
    }

    private IEnumerator PanCamera()
    {
        var framingTransposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();

        framingTransposer.m_XDamping = cameraDamping;
        framingTransposer.m_YDamping = cameraDamping;

        framingTransposer.m_DeadZoneWidth = 0f;

        framingTransposer.m_TrackedObjectOffset = Vector3.zero;

        yield return StartCoroutine(ZoomIn(zoomAmount, zoomDuration));
    }

    private IEnumerator ZoomIn(float targetSize, float duration)
    {
        float startSize = virtualCamera.m_Lens.OrthographicSize;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            virtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(startSize, targetSize, t);
            yield return null;
        }

        virtualCamera.m_Lens.OrthographicSize = targetSize;
    }

    private void DisintegratePlayer()
    {
        var de = player.GetComponentInChildren<DisintegrationEffect>();
        if (de != null)
        {
            de.Disintegrate();
        }

        var sr = player.GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            sr.enabled = false;
        }

        var afterimage = player.GetComponentInChildren<Afterimage>();
        if (afterimage != null)
        {
            afterimage.DestroyAfterimages();
        }
    }

    private IEnumerator RampUpNoise(float targetAmplitude, float duration)
    {
        float startAmplitude = noise.m_AmplitudeGain;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            noise.m_AmplitudeGain = Mathf.Lerp(startAmplitude, targetAmplitude, t);
            yield return null;
        }

        noise.m_AmplitudeGain = targetAmplitude;
    }

    private void StopNoise()
    {
        noise.m_AmplitudeGain = 0f;
    }
}
