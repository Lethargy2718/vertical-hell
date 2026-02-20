using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcessController : MonoBehaviour
{
    public static PostProcessController Instance { get; private set; }

    private Volume _volume;
    private Vignette _vignette;
    private ChromaticAberration _aberration;

    private readonly List<PostProcessModifier> _modifiers = new();

    public void AddModifier(PostProcessModifier modifier) => _modifiers.Add(modifier);
    public void RemoveModifier(PostProcessModifier modifier) => _modifiers.Remove(modifier);

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        _volume = GetComponent<Volume>();
        _volume.profile.TryGet(out _vignette);
        _volume.profile.TryGet(out _aberration);

        _modifiers.Add(new PostProcessModifier
        {
            vignetteOffset = _vignette != null ? _vignette.intensity.value : 0f,
            chromaticOffset = _aberration != null ? _aberration.intensity.value : 0f,
            vignetteColor = Color.black
        });
    }

    private void Update()
    {
        float vignette = 0f;
        float chromatic = 0f;
        Color weightedColor = Color.black;
        float totalWeight = 0f;

        foreach (var mod in _modifiers)
        {
            vignette += mod.vignetteOffset;
            chromatic += mod.chromaticOffset;

            weightedColor += mod.vignetteColor * mod.vignetteOffset;
            totalWeight += mod.vignetteOffset;
        }

        if (_vignette != null)
        {
            _vignette.intensity.value = Mathf.Clamp01(vignette);
            _vignette.color.value = totalWeight > 0 ? weightedColor / totalWeight : Color.black;
        }

        if (_aberration != null)
            _aberration.intensity.value = Mathf.Clamp01(chromatic);
    }
}

public class PostProcessModifier
{
    public float vignetteOffset;
    public float chromaticOffset;
    public Color vignetteColor = Color.black;
}