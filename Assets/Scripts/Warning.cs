using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

[DefaultExecutionOrder(1000)]
public class Warning : MonoBehaviour
{
    [SerializeField] private float maxIntensity = 1.0f;
    [SerializeField] private AnimationCurve fadeInCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private AnimationCurve fadeOutCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    private Light2D _light;

    private void Awake()
    {
        _light = GetComponentInChildren<Light2D>();
    }

    private void LateUpdate()
    {
        transform.SetY(LevelBounds.Instance.CameraTopY);
    }

    public void PulseAndDestroy(float duration, float waitDuration,float fadeOutDuration)
    {
        _light.intensity = 0f;
        StartCoroutine(PulseAndDestroyCoroutine(duration, waitDuration, fadeOutDuration));
    }

    private IEnumerator PulseAndDestroyCoroutine(float duration, float waitDuration, float fadeOutDuration)
    {
        float fadeInDuration = duration - fadeOutDuration;

        // Fade in
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            float t = elapsed / fadeInDuration;
            _light.intensity = fadeInCurve.Evaluate(t) * maxIntensity;
            elapsed += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(waitDuration);

        // Fade out
        elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            float t = elapsed / fadeOutDuration;
            _light.intensity = fadeOutCurve.Evaluate(t) * maxIntensity;
            elapsed += Time.deltaTime;
            yield return null;
        }

        _light.intensity = 0f;
        Destroy(gameObject);
    }
}