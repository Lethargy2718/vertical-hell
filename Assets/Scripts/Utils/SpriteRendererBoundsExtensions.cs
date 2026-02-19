using UnityEngine;

public static class SpriteRendererBoundsExtensions
{
    public static float Top(this SpriteRenderer sr) => sr.bounds.max.y;

    public static float Bottom(this SpriteRenderer sr) => sr.bounds.min.y;

    public static float Left(this SpriteRenderer sr) => sr.bounds.min.x;

    public static float Right(this SpriteRenderer sr) => sr.bounds.max.x;

    public static float Width(this SpriteRenderer sr) => sr.bounds.size.x;

    public static float Height(this SpriteRenderer sr) => sr.bounds.size.y;
}
