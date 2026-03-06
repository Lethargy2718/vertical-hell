using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class RectElement : MonoBehaviour
{
    private SpriteRenderer _sr;

    protected void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
    }

    public void Initialize(float width, float height, Color? color = null)
    {
        transform.localScale = new Vector3(width, height, 1f);

        if (color.HasValue)
            _sr.color = color.Value;
    }
}
