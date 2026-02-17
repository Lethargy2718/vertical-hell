using Unity.VisualScripting;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("Walls")]
    [SerializeField] private Wall wallPrefab;
    [SerializeField] private float wallWidth;
    [SerializeField] private float wallHeight;
    [SerializeField] private Color wallColor;

    [Header("Platforms")]
    [SerializeField] private Platform platformPrefab;
    [SerializeField] private float platformWidth;
    [SerializeField] private float platformHeight;
    [SerializeField] private Color platformColor;
    [SerializeField] private int platformCount = 10;
    [SerializeField] private float firstPlatformY = 2.5f;
    [SerializeField] private float distanceBetweenPlatforms = 2f;

    private enum Direction { Left, Right };

    private LevelBounds LB => LevelBounds.Instance;
    
    private void Start()
    {
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
        return Spawn<Wall>(wallPrefab, centerX, wallHeight / 2, wallWidth, wallHeight, wallColor);
    }

    private void SpawnPlatforms()
    {
        SpawnPlatformsOnSides();
    }

    private void SpawnPlatformsOnSides()
    {
        for (int i = 0; i < platformCount; i++)
        {
            float x;

            if (i % 2 == 0) // Left
            {
                x = LB.LeftWallX + platformWidth / 2;
            }
            else
            {
                x = LB.RightWallX - platformWidth / 2;
            }
    
            float y = firstPlatformY + i * distanceBetweenPlatforms;

            SpawnPlatform(x, y);
        }
        Debug.Log(LB.LeftWallX);
    }

    private Platform SpawnPlatform(float x, float y)
    {
        return Spawn<Platform>(platformPrefab, x, y, platformWidth, platformHeight, platformColor);
    }

    public T Spawn<T>(T prefab, float x, float y, float width, float height, Color color) where T : RectElement
    {
        T instance = Instantiate(prefab);
        instance.Initialize(width, height, color);
        instance.transform.position = new Vector3(x, y, 0f);
        return instance;
    }
}
