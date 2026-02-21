using System.Collections;
using UnityEngine;

public class Warning : MonoBehaviour
{
    public float duration = 1.0f;

    private void LateUpdate()
    {
        transform.SetY(LevelBounds.Instance.CameraTopY);
    }
}
