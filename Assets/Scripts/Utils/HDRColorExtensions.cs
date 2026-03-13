using UnityEngine;

public static class HDRColorExtensions
{
    private const float MAX_BASE_HDR_VALUE = 191f / 255f; // From Unity's internals

    public static (Color baseColor, float intensity) Decompose(this Color color)
    {
        float maxColorComponent = color.maxColorComponent;

        if (maxColorComponent == 0f)
            return (Color.black, 0f);

        float scaleFactor = MAX_BASE_HDR_VALUE / maxColorComponent;
        float intensity = Mathf.Log(1f / scaleFactor, 2f);
        Color baseColor = new Color(
            color.r * scaleFactor,
            color.g * scaleFactor,
            color.b * scaleFactor,
            color.a
        );
        return (baseColor, intensity);
    }

    public static Color WithIntensity(this Color color, float intensity)
    {
        var (baseColor, _) = color.Decompose();
        float multiplier = Mathf.Pow(2, intensity);

        return new Color(
            baseColor.r * multiplier,
            baseColor.g * multiplier,
            baseColor.b * multiplier,
            color.a
        );
    }
}