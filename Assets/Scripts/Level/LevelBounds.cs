using UnityEngine;
using Cinemachine;

public class LevelBounds : MonoBehaviour
{
    public static LevelBounds Instance { get; private set; }

    // Base settings
    private const float baseWidth = 640f;
    private const float baseHeight = 360f;
    private const float ppu = 32f;

    public const float fullHorizontalTiles = 20;
    public const float horizontalTiles = 18;

    // Camera
    [SerializeField] private CinemachineVirtualCamera vcam;
    private Camera cam;
    public float OrthoSize => baseHeight / ppu / 2;
    public float CameraTopY => cam.ScreenToWorldPoint(new Vector3(0, Screen.height, 0)).y;
    public float CameraBottomY => cam.ScreenToWorldPoint(Vector3.zero).y;
    public float CameraLeftX => -(baseWidth / 2) / ppu;
    public float CameraRightX => -CameraLeftX;

    // Walls
    public float WallOffset => (fullHorizontalTiles - horizontalTiles) / 2;
    public float LeftWallX => CameraLeftX + WallOffset;
    public float RightWallX => CameraRightX - WallOffset;



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

    private void Start()
    {
        vcam.m_Lens.OrthographicSize = OrthoSize;
    }

    public float GetLeftWallCenterX(float wallWidth)
    {
        return CameraLeftX - wallWidth / 2 + WallOffset;
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
