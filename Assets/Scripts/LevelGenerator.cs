using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Transform player;

    [Header("Walls")]
    [SerializeField] private Wall wallPrefab;
    [SerializeField] private Color wallColor;
    [SerializeField] private float wallExtraPart;
    [SerializeField] private float wallWidth;
    public float wallHeight;
    private GameObject _wallContainer;

    [Header("Platforms")]
    [SerializeField] private Platform platformPrefab;
    [SerializeField] private float minPlatformWidth;
    [SerializeField] private float maxPlatformWidth;
    [SerializeField] private float platformHeight;
    [SerializeField] private Color platformColor;
    [SerializeField] private int platformCount = 10;
    [SerializeField] private float firstPlatformY = 2.5f;
    [SerializeField] private float minDistanceBetweenPlatforms = 1.5f;
    [SerializeField] private float maxDistanceBetweenPlatforms = 2.3f;
    private float DistanceBetweenPlatforms => Random.Range(minDistanceBetweenPlatforms, maxDistanceBetweenPlatforms);
    private float PlatformWidth => Random.Range(minPlatformWidth, maxPlatformWidth);
    private GameObject _platformContainer;
    private LevelBounds LB => LevelBounds.Instance;

    private void Start()
    {
        _wallContainer = new GameObject("Walls");
        _platformContainer = new GameObject("Platforms");

        SpawnWalls();
        SpawnPlatforms();
    }

    private void SpawnWalls()
    {
        float leftX = LB.GetLeftWallCenterX(wallWidth);
        float rightX = LB.GetRightWallCenterX(wallWidth);

        SpawnWall(leftX);
        SpawnWall(rightX);
    }

    private Wall SpawnWall(float centerX)
    {
        float heightWithExtra = wallHeight + wallExtraPart;
        float centerY = heightWithExtra / 2 - wallExtraPart;

        return Spawn<Wall>(wallPrefab, centerX, centerY, wallWidth, heightWithExtra, wallColor, _wallContainer.transform);
    }

    private void SpawnPlatforms()
    {
        SpawnPlatformsOnSides();
    }

    private void SpawnPlatformsOnSides()
    {
        float currentPos = 0;

        for (int i = 0; i < platformCount; i++)
        {
            float x;
            float platformWidth = PlatformWidth;

            //if (i % 2 == 0) // Left
            //{
            //    x = LB.LeftWallX + platformWidth / 2;
            //}
            //else
            //{
            //    x = LB.RightWallX - platformWidth / 2;
            //}

            x = LB.GetRandomX(platformWidth);

            float dist;
            if (i == 0) dist = firstPlatformY;
            else dist = DistanceBetweenPlatforms;

            float y = currentPos + dist;
            currentPos += dist;

            SpawnPlatform(x, y, platformWidth);
        }
    }

    private Platform SpawnPlatform(float x, float y, float platformWidth)
    {
        return Spawn<Platform>(platformPrefab, x, y, platformWidth, platformHeight, platformColor, _platformContainer.transform);
    }

    public T Spawn<T>(T prefab, float x, float y, float width, float height, Color color, Transform parent = null) where T : RectElement
    {
        T instance = Instantiate(prefab, parent);
        instance.Initialize(width, height, color);
        instance.transform.position = new Vector3(x, y, 0f);
        return instance;
    }
}
