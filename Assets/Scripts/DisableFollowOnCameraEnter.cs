using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class DisableFollowOnCameraEnter : MonoBehaviour
{
    private FollowX _followX;
    private SpriteRenderer _sr;

    private LevelBounds LB => LevelBounds.Instance;

    private void Awake()
    {
        _followX = GetComponent<FollowX>();
        _sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (InCameraBounds)
        {
            Destroy(_followX);
            Destroy(this);
        }
    }

    private bool InCameraBounds
    {
        get
        {
            float bottomY = _sr.Bottom();
            float topY = _sr.Top();
            return bottomY <= LB.CameraTopY && topY >= LB.CameraBottomY;
        }
    }
}
