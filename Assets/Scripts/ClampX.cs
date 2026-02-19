using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ClampX : MonoBehaviour
{
    private SpriteRenderer _sr;
    private LevelBounds LB => LevelBounds.Instance;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        Clamp();
    }

    private void Clamp()
    {
        float halfWidth = _sr.Width() / 2f;
        float newX = Mathf.Clamp(transform.position.x, LB.LeftWallX + halfWidth, LB.RightWallX - halfWidth);
        transform.SetX(newX);
    }
}
