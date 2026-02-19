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

        var baseline = new PostProcessModifier
        {
            vignetteOffset = _vignette.intensity.value,
            chromaticOffset = _aberration.intensity.value
        }
        ;
        _modifiers.Add(baseline);
    }

    private void Update()
    {
        float vignette = 0f;
        float chromatic = 0f;

        foreach (var mod in _modifiers)
        {
            vignette += mod.vignetteOffset;
            chromatic += mod.chromaticOffset;
        }

        _vignette.intensity.value = Mathf.Clamp01(vignette);
        _aberration.intensity.value = Mathf.Clamp01(chromatic);
    }
}

public class PostProcessModifier
{
    public float vignetteOffset;
    public float chromaticOffset;
}