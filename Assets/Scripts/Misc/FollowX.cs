using UnityEngine;

public class FollowX : MonoBehaviour
{
    public Transform followTarget;

    private void LateUpdate()
    {
        if (followTarget == null) return;

        transform.SetX(followTarget.position.x);
    }
}
