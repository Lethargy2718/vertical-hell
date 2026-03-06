using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private FloatingEnemy floatingEnemyPrefab; // will add IEnemy or whatever later
    [SerializeField] private float breakDuration;
    [SerializeField] private float spawnOffsetFromCameraTop = 3f;

    private int wave = 1;

    private int _enemyCount = 0;
    private int EnemyCount
    {
        get => _enemyCount;
        set
        {
            _enemyCount = value;
            if (_enemyCount == 0)
            {
                //wave++;
                SpawnEnemies(wave);
            }
        }
    }

    private LevelBounds LB => LevelBounds.Instance;


    private void Start()
    {
        SpawnEnemies(wave);
    }

    private void SpawnEnemies(int count)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnEnemy();
        }
    }

    private void SpawnEnemy()
    {
        FloatingEnemy enemy = Instantiate(floatingEnemyPrefab);
        enemy.transform.SetX(LB.GetRandomX());
        enemy.transform.SetY(LB.CameraTopY + spawnOffsetFromCameraTop);
        enemy.player = player;
        
        if (enemy.TryGetComponent<ProjectileShooter>(out var projectileShooter))
        {
            projectileShooter.target = player;
        }

        if (enemy.TryGetComponent<HealthComponent>(out var healthComponent))
        {
            healthComponent.HealthDepleted += () => EnemyCount--;
        }

        EnemyCount++;
    }
}
