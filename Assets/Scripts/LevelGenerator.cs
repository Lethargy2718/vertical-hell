using System.Collections;
using System.Collections.Generic;
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

    [Header("Spikes")]
    [SerializeField] private FallingSpike fallingSpikePrefab;
    [SerializeField] private float fallingSpikeSpacing = 1.0f;
    [SerializeField] private float fallingSpikeSpeed = 1.0f;
    [SerializeField] private int skippedPreGeneratedSpikes = 3;
    private float FallingSpikeSpawnInterval => (fallingSpikePrefab.transform.localScale.y + fallingSpikeSpacing) / fallingSpikeSpeed;
    private Coroutine fallingSpikeSpawnRoutine;
    private GameObject fallingSpikeContainer;

    private enum Direction { Left, Right };

    private LevelBounds LB => LevelBounds.Instance;
    
    private void Start()
    {
        SpawnWalls();

        SpawnPlatforms();

        fallingSpikeContainer = new GameObject("Spikes");
        StartGeneratingFallingSpikes();
    }

    private void Update()
    {
        // debug
        if (Input.GetKeyDown(KeyCode.F))
        {
            StopGeneratingFallingSpikes();
        }
        else if (Input.GetKeyDown(KeyCode.G))
        {
            StartGeneratingFallingSpikes();
        }
    }

    #region Walls & Platforms

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

    #endregion

    #region Falling Spikes

    private FallingSpike SpawnSpike(float x, float y)
    {
        FallingSpike fallingSpike = Instantiate(fallingSpikePrefab, fallingSpikeContainer.transform);

        if (fallingSpike.TryGetComponent<MoveDown>(out var moveDown))
        {
            moveDown.Speed = fallingSpikeSpeed;
        }

        Vector3 spikePos = new Vector3(x, y, 0f);
        fallingSpike.transform.position = spikePos;
        return fallingSpike;
    }

    private IEnumerator SpawnFallingSpikesCoroutine()
    {
        while (true)
        {
            SpawnSpike(LB.MidX, wallHeight);
            yield return new WaitForSeconds(FallingSpikeSpawnInterval);
        }
    }

    private void SpawnPreGeneratedFallingSpikes()
    {
        float spikeHeight = fallingSpikePrefab.transform.localScale.y;
        float step = spikeHeight + fallingSpikeSpacing;
        float bottomLimit = Camera.main.ScreenToWorldPoint(Vector3.zero).y + step * skippedPreGeneratedSpikes;

        for (float y = wallHeight - step; y >= bottomLimit; y -= step)
        {
            FallingSpike spike = SpawnSpike(LB.MidX, y);
        }
    }

    private void DestroyFallingSpikesOutOfCamera()
    {
        foreach (Transform child in fallingSpikeContainer.transform)
        {
            float childTop = child.transform.position.y + child.transform.localScale.y / 2;
            float childBottom = child.transform.position.y - child.transform.localScale.y / 2;

            if (childTop < LB.CameraBottomY || childBottom > LB.CameraTopY)
            {
                Destroy(child.gameObject);
            }
        }
    }

    private void StartGeneratingFallingSpikes()
    {
        SpawnPreGeneratedFallingSpikes();
        fallingSpikeSpawnRoutine = StartCoroutine(SpawnFallingSpikesCoroutine());
    }

    private void StopGeneratingFallingSpikes()
    {
        StopCoroutine(fallingSpikeSpawnRoutine);
        DestroyFallingSpikesOutOfCamera();
    }

    #endregion
}
