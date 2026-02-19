using System.Collections;
using UnityEngine;

public class Warning : MonoBehaviour
{
    private FollowX _followX;
    public float duration = 1.0f;

    private void Awake()
    {
        _followX = GetComponent<FollowX>();
    }

    public void Follow(Transform t)
    {
        _followX.followTarget = t;
    }

    private void LateUpdate()
    {
        transform.SetY(LevelBounds.Instance.CameraTopY);
    }
}
