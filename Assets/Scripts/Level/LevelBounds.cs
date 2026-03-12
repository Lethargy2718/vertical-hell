using UnityEngine;
using Cinemachine;

public class LevelBounds : MonoBehaviour
{
    public static LevelBounds Instance { get; private set; }

    // TODO: decide on a single source of truth for both camera script and this
    private const float baseWidth = 640f;
    private const float baseHeight = 360f;
    private const float ppu = 32f;

    private const float fullHorizontalTiles = baseWidth / ppu;      
    private const float horizontalTiles = 18f;                     

    [SerializeField] private CinemachineVirtualCamera vcam;
    private Camera cam;

    public float CameraTopY => cam.transform.position.y + cam.orthographicSize;
    public float CameraBottomY => cam.transform.position.y - cam.orthographicSize;
    public float CameraLeftX => cam.transform.position.x - cam.orthographicSize * cam.aspect;
    public float CameraRightX => cam.transform.position.x + cam.orthographicSize * cam.aspect;

    private float CameraDefaultLeftX => -fullHorizontalTiles / 2f;
    private float CameraDefaultRightX => fullHorizontalTiles / 2f; 
    private float WallOffset => (fullHorizontalTiles - horizontalTiles) / 2f;

    public float LeftWallX => CameraDefaultLeftX + WallOffset;
    public float RightWallX => CameraDefaultRightX - WallOffset;

    public float OrthoSize => baseHeight / ppu / 2f;

    public float MidX => (LeftWallX + RightWallX) / 2f;
    public float MidY => (CameraBottomY + CameraTopY) / 2;

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
        if (vcam != null)
            vcam.m_Lens.OrthographicSize = OrthoSize;
    }

    /// <summary>
    /// Returns the world X coordinate for the center of a left wall of given width,
    /// so that its right edge aligns with LeftWallX.
    /// </summary>
    public float GetLeftWallCenterX(float wallWidth)
    {
        return LeftWallX - wallWidth / 2f;
    }

    /// <summary>
    /// Returns the world X coordinate for the center of a right wall of given width,
    /// so that its left edge aligns with RightWallX.
    /// </summary>
    public float GetRightWallCenterX(float wallWidth)
    {
        return RightWallX + wallWidth / 2f;
    }

    /// <summary>
    /// Divides the playable width (RightWallX - LeftWallX) into partitionCount equal parts
    /// and returns the X coordinate of the center of the partition at partitionIndex (0-indexed).
    /// </summary>
    public float GetPartitionIndexX(int partitionCount, int partitionIndex)
    {
        float usableWidth = RightWallX - LeftWallX;
        float partitionWidth = usableWidth / partitionCount;
        return LeftWallX + partitionWidth * (partitionIndex + 0.5f);
    }

    /// <summary>
    /// Returns an array of all partition center X coordinates for the given partition count.
    /// </summary>
    public float[] GetPartitionIndicesX(int partitionCount)
    {
        float[] indices = new float[partitionCount];
        for (int i = 0; i < partitionCount; i++)
            indices[i] = GetPartitionIndexX(partitionCount, i);
        return indices;
    }

    /// <summary>
    /// Returns a random X coordinate within the playable area, optionally accounting for object width and offset.
    /// </summary>
    public float GetRandomX(float objectWidth = 0f, float offset = 0f)
    {
        float minX = LeftWallX + objectWidth / 2f + offset;
        float maxX = RightWallX - objectWidth / 2f - offset;

        if (minX > maxX)
            return MidX;

        return Random.Range(minX, maxX);
    }

    /// <summary>
    /// Returns a random Y coordinate within the playable area, optionally accounting for object width and offset.
    /// </summary>
    public float GetRandomY(float objectHeight = 0f, float offset = 0f)
    {
        float minY = CameraBottomY + objectHeight / 2f + offset;
        float maxY = CameraTopY - objectHeight / 2f - offset;

        if (minY > maxY)
            return MidY;

        return Random.Range(minY, maxY);
    }
}