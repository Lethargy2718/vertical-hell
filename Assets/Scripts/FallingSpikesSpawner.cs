using UnityEngine;
using System.Collections;

public class FallingSpikesSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LevelGenerator levelGenerator;
    [SerializeField] private Transform player;

    [Header("Spikes")]
    [SerializeField] private FallingSpike fallingSpikePrefab;
    [SerializeField] private float fallingSpikeSpacing = 1.0f;
    [SerializeField] private float fallingSpikeSpeed = 1.0f;
    [SerializeField] private int skippedPreGeneratedSpikes = 3;
    [SerializeField] private GameObject warningPrefab;
    private Warning _warning;
    private float FallingSpikeSpawnInterval => (fallingSpikePrefab.transform.localScale.y + fallingSpikeSpacing) / fallingSpikeSpeed;
    private Coroutine fallingSpikeSpawnRoutine;
    private GameObject fallingSpikeContainer;
    private bool _spawningFallingSpikes = false;
    private LevelBounds LB => LevelBounds.Instance;


    private void Start()
    {
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
            SpawnSpike(fallingSpikePrefab, LB.MidX, levelGenerator.wallHeight);
            yield return new WaitForSeconds(FallingSpikeSpawnInterval);
        }
    }

    private void SpawnMiddlePreGeneratedFallingSpikes()
    {
        float spikeHeight = fallingSpikePrefab.transform.localScale.y;
        float step = spikeHeight + fallingSpikeSpacing;
        float bottomLimit = Camera.main.ScreenToWorldPoint(Vector3.zero).y + step * skippedPreGeneratedSpikes;

        for (float y = levelGenerator.wallHeight - step; y >= bottomLimit; y -= step)
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
            _warning = SpawnWarning(phaseDuration * 2f, 0f, 0.2f);
            FollowX followX = _warning.gameObject.AddComponent<FollowX>();
            followX.followTarget = player;
            yield return new WaitForSeconds(phaseDuration);

            // Lock warning at this position
            Destroy(followX);
            float finalX = _warning.transform.position.x;
            yield return new WaitForSeconds(phaseDuration);

            // Destroy warning and spawn spike at that final pos
            SpawnSpike(fallingSpikePrefab, finalX, LB.CameraTopY + spikeHeight / 2);

            // Wait for next spike
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
            _warning = SpawnWarning(phaseDuration, 0f, 0.2f);
            _warning.transform.SetX(randomX);
            yield return new WaitForSeconds(phaseDuration);

            // Spawn spike at that final pos
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

    private Warning SpawnWarning(float warningDuration, float waitDuration, float fadeOutDuration)
    {
        GameObject warning = Instantiate(warningPrefab);

        warning.transform.SetY(LB.CameraTopY);

        if (warning.TryGetComponent<Warning>(out var warningComponent))
        {
            warningComponent.PulseAndDestroy(warningDuration, waitDuration, fadeOutDuration);
            return warningComponent;
        }

        return null;
    }

    #endregion
}
