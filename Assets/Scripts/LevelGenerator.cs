using System.Collections;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Transform player;

    [Header("Walls")]
    [SerializeField] private Wall wallPrefab;
    [SerializeField] private float wallWidth;
    [SerializeField] private float wallHeight;
    [SerializeField] private float wallExtraPart;
    [SerializeField] private Color wallColor;

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

    [Header("Spikes")]
    [SerializeField] private FallingSpike fallingSpikePrefab;
    [SerializeField] private FallingSpike fallingSpikeFollowPrefab;
    [SerializeField] private float fallingSpikeSpacing = 1.0f;
    [SerializeField] private float fallingSpikeSpeed = 1.0f;
    [SerializeField] private int skippedPreGeneratedSpikes = 3;
    [SerializeField] private GameObject warningPrefab;
    private Warning _warning;
    private float FallingSpikeSpawnInterval => (fallingSpikePrefab.transform.localScale.y + fallingSpikeSpacing) / fallingSpikeSpeed;
    private Coroutine fallingSpikeSpawnRoutine;
    private GameObject fallingSpikeContainer;
    private bool _spawningFallingSpikes = false;

    private enum Direction { Left, Right };

    private LevelBounds LB => LevelBounds.Instance;

    private void Start()
    {
        SpawnWalls();

        SpawnPlatforms();

        fallingSpikeContainer = new GameObject("Spikes");
        StartGeneratingMiddleFallingSpikes();
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
            StopGeneratingFallingSpikes();
            StartGeneratingMiddleFallingSpikes();
        }
        else if (Input.GetKeyDown(KeyCode.H))
        {
            StopGeneratingFallingSpikes();
            StartGeneratingFollowFallingSpikes();
        }
        else if (Input.GetKeyDown(KeyCode.J))
        {
            StopGeneratingFallingSpikes();
            StartGeneratingRandomFallingSpikes();
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
        float heightWithExtra = wallHeight + wallExtraPart;
        float centerY = heightWithExtra / 2 - wallExtraPart;

        return Spawn<Wall>(wallPrefab, centerX, centerY, wallWidth, heightWithExtra, wallColor);
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

    private FallingSpike SpawnSpike(FallingSpike spikePrefab, float x, float y)
    {
        FallingSpike fallingSpike = Instantiate(spikePrefab, fallingSpikeContainer.transform);

        if (fallingSpike.TryGetComponent<MoveDown>(out var moveDown))
        {
            moveDown.Speed = fallingSpikeSpeed;
        }

        Vector3 spikePos = new Vector3(x, y, 0f);
        fallingSpike.transform.position = spikePos;

        return fallingSpike;
    }

    // TODO: Refactor into separate files

    #region Middle Falling Spikes

    private void StartGeneratingMiddleFallingSpikes()
    {
        if (_spawningFallingSpikes) return;
        SpawnMiddlePreGeneratedFallingSpikes();
        fallingSpikeSpawnRoutine = StartCoroutine(SpawnMiddleFallingSpikesCoroutine());
        _spawningFallingSpikes = true;
    }

    private IEnumerator SpawnMiddleFallingSpikesCoroutine()
    {
        while (true)
        {
            SpawnSpike(fallingSpikePrefab, LB.MidX, wallHeight);
            yield return new WaitForSeconds(FallingSpikeSpawnInterval);
        }
    }

    private void SpawnMiddlePreGeneratedFallingSpikes()
    {
        float spikeHeight = fallingSpikePrefab.transform.localScale.y;
        float step = spikeHeight + fallingSpikeSpacing;
        float bottomLimit = Camera.main.ScreenToWorldPoint(Vector3.zero).y + step * skippedPreGeneratedSpikes;

        for (float y = wallHeight - step; y >= bottomLimit; y -= step)
        {
            SpawnSpike(fallingSpikePrefab, LB.MidX, y);
        }
    }

    #endregion

    #region Following Falling Spikes

    private void StartGeneratingFollowFallingSpikes()
    {
        if (_spawningFallingSpikes) return;
        fallingSpikeSpawnRoutine = StartCoroutine(SpawnFollowingFallingSpikesCoroutine());
        _spawningFallingSpikes = true;
    }

    private IEnumerator SpawnFollowingFallingSpikesCoroutine()
    {
        float spikeHeight = fallingSpikePrefab.GetComponent<SpriteRenderer>().Height();

        while (true)
        {
            float phaseDuration = FallingSpikeSpawnInterval * 1f / 3f;

            // Show warning that follows player
            _warning = SpawnWarning(phaseDuration);
            FollowX followX = _warning.gameObject.AddComponent<FollowX>();
            followX.followTarget = player;
            yield return new WaitForSeconds(phaseDuration);

            // Lock warning at this position
            Destroy(followX);
            float finalX = _warning.transform.position.x;
            yield return new WaitForSeconds(phaseDuration);

            // Destroy warning and spawn spike at that final pos
            Destroy(_warning.gameObject);
            SpawnSpike(fallingSpikePrefab, finalX, LB.CameraTopY + spikeHeight / 2);
            yield return new WaitForSeconds(phaseDuration);
        }
    }

    #endregion

    #region Random Falling Spikes

    private void StartGeneratingRandomFallingSpikes()
    {
        if (_spawningFallingSpikes) return;
        fallingSpikeSpawnRoutine = StartCoroutine(SpawnRandomFallingSpikesCoroutine());
        _spawningFallingSpikes = true;
    }

    private IEnumerator SpawnRandomFallingSpikesCoroutine()
    {
        SpriteRenderer sr = fallingSpikePrefab.GetComponent<SpriteRenderer>();
        float spikeWidth = sr.Width();
        float spikeHeight = sr.Height();

        while (true)
        {
            float phaseDuration = FallingSpikeSpawnInterval * 1f / 2f;

            // Show warning 
            float randomX = LB.GetRandomX(spikeWidth);
            _warning = SpawnWarning(phaseDuration);
            _warning.transform.SetX(randomX);
            yield return new WaitForSeconds(phaseDuration);

            // Destroy warning and spawn spike at that final pos
            Destroy(_warning.gameObject);
            SpawnSpike(fallingSpikePrefab, randomX, LB.CameraTopY + spikeHeight / 2);
            yield return new WaitForSeconds(phaseDuration);
        }
    }

    #endregion

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

    private void StopGeneratingFallingSpikes()
    {
        if (!_spawningFallingSpikes) return;
        StopCoroutine(fallingSpikeSpawnRoutine);
        if (_warning != null) Destroy(_warning.gameObject);
        DestroyFallingSpikesOutOfCamera();
        _spawningFallingSpikes = false;
        fallingSpikeSpawnRoutine = null;
    }

    private Warning SpawnWarning(float warningDuration)
    {
        GameObject warning = Instantiate(warningPrefab);

        warning.transform.SetY(LB.CameraTopY);

        if (warning.TryGetComponent<Warning>(out var warningComponent))
        {
            warningComponent.duration = warningDuration;
            return warningComponent;
        }

        return null;
    }

    #endregion
}
