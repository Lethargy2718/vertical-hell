using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    public Transform target;
    public float verticalOffset = 0f;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        if (target != null)
        {
            Vector3 pos = transform.position;
            pos.y = Mathf.MoveTowards(pos.y, target.position.y + verticalOffset, 0.10f);
            transform.position = new Vector3(pos.x, pos.y, pos.z);
        }
    }

    public float LeftEdge => transform.position.x - cam.orthographicSize * 2f * cam.aspect / 2f;
    public float RightEdge => transform.position.x + cam.orthographicSize * 2f * cam.aspect / 2f;
    public float TopEdge => transform.position.y + cam.orthographicSize;
    public float BottomEdge => transform.position.y - cam.orthographicSize;
}
