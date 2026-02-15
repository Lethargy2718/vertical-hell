using UnityEngine;

[ExecuteAlways]
public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    public float fixedVerticalHeight = 10f; // World units visible vertically
    public Transform target;
    public float verticalOffset = 0f;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (!cam.orthographic)
        {
            Debug.LogWarning("CameraController requires an Orthographic Camera!");
            cam.orthographic = true;
        }
    }

    private void LateUpdate()
    {
        cam.orthographicSize = fixedVerticalHeight / 2f;

        if (target != null)
        {
            Vector3 pos = transform.position;
            pos.y = target.position.y + verticalOffset;
            transform.position = new Vector3(pos.x, pos.y, pos.z);
        }
    }

    public float LeftEdge => transform.position.x - cam.orthographicSize * 2f * cam.aspect / 2f;
    public float RightEdge => transform.position.x + cam.orthographicSize * 2f * cam.aspect / 2f;
    public float TopEdge => transform.position.y + cam.orthographicSize;
    public float BottomEdge => transform.position.y - cam.orthographicSize;


    void PlaceWalls(Transform leftWall, Transform rightWall)
    {
        float wallHalfWidth = leftWall.localScale.x / 2f;

        leftWall.position = new Vector3(LeftEdge + wallHalfWidth, leftWall.position.y, leftWall.position.z);
        rightWall.position = new Vector3(RightEdge - wallHalfWidth, rightWall.position.y, rightWall.position.z);

        float vertHeight = cam.orthographicSize * 2f;
        leftWall.localScale = new Vector3(leftWall.localScale.x, vertHeight, leftWall.localScale.z);
        rightWall.localScale = new Vector3(rightWall.localScale.x, vertHeight, rightWall.localScale.z);
    }

}
