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
    private Camera cam;
    public float CameraTopY => cam.ScreenToWorldPoint(new Vector3(0, Screen.height, 0)).y;
    public float CameraBottomY => cam.ScreenToWorldPoint(Vector3.zero).y;
    public float CameraLeftX => -(_BASE_WIDTH / 2) / _PPU;
    public float CameraRightX => -CameraLeftX;

    // Walls
    public float LeftWallX => CameraLeftX + _WALL_OFFSET;
    public float RightWallX => CameraRightX - _WALL_OFFSET;


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

    public float GetLeftWallCenterX(float wallWidth)
    {
        return CameraLeftX - wallWidth / 2 + _WALL_OFFSET;
    }

    public float GetRightWallCenterX(float wallWidth)
    {
        return -GetLeftWallCenterX(wallWidth);
    }

    public float GetPartitionIndexX(int partitionCount, int partitionIndex)
    {
        float usableWidth = RightWallX - LeftWallX;
        float partitionWidth = usableWidth / partitionCount;
        return LeftWallX + partitionWidth * (partitionIndex + 0.5f);
    }

    public float[] GetPartitionIndicesX(int partitionCount)
    {
        float[] indices = new float[partitionCount];

        for (int i = 0; i < partitionCount; i++)
        {
            indices[i] = GetPartitionIndexX(partitionCount, i);
        }

        return indices;
    }

    public float MidX => (LeftWallX + RightWallX) / 2f; // Always 0 anyways but it's better off dynamic

    public float GetRandomX(float objectWidth = 0f, float offset = 0f)
    {
        float minX = LeftWallX + objectWidth + offset;
        float maxX = RightWallX - objectWidth - offset;

        return Random.Range(minX, maxX);
    }
}
