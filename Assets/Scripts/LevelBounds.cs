using UnityEngine;

public class LevelBounds : MonoBehaviour
{
    public static LevelBounds Instance { get; private set; }

    // Base settings
    private const float _BASE_WIDTH = 180f;
    private const float _BASE_HEIGHT = 320f;
    private const float _PPU = 32f;
    private const float _WALL_OFFSET = 0.3125f;

    // Camera
    public float CameraTopY { get; private set; }
    public float CameraLeftX => -(_BASE_WIDTH / 2) / _PPU;
    public float CameraRightX => -CameraLeftX;

    // Walls
    public float LeftWallX => CameraLeftX + _WALL_OFFSET;
    public float RightWallX => CameraRightX - _WALL_OFFSET;

    private Camera cam;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        cam = Camera.main;
    }

    private void LateUpdate()
    {
        float visibleHeight = cam.orthographicSize * 2;
        CameraTopY = cam.transform.position.y + visibleHeight / 2;
    }

    public float GetLeftWallCenterX(float wallWidth)
    {
        return CameraLeftX - wallWidth / 2 + _WALL_OFFSET;
    }

    public float GetRightWallCenterX(float wallWidth)
    {
        return -GetLeftWallCenterX(wallWidth);
    }
}
