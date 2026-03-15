using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Pool;

public class FlyweightFactory : MonoBehaviour
{
    [SerializeField] bool collectionCheck = true;
    [SerializeField] int defaultCapacity = 10;
    [SerializeField] int maxPoolSize = 100;

    static FlyweightFactory instance;
    readonly Dictionary<FlyweightType, IObjectPool<Flyweight>> pools = new();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static Flyweight Spawn(FlyweightSettings settings) => instance.GetPoolFor(settings)?.Get();
    public static void ReturnToPool(Flyweight f) => instance.GetPoolFor(f.settings)?.Release(f);

    IObjectPool<Flyweight> GetPoolFor(FlyweightSettings settings)
    {
        IObjectPool<Flyweight> pool;

        if (pools.TryGetValue(settings.type, out pool)) return pool;

        pool = new ObjectPool<Flyweight>(
            settings.Create,
            settings.OnGet,
            settings.OnRelease,
            settings.OnDestroyPoolObject,
            collectionCheck,
            defaultCapacity,
            maxPoolSize
        );
        pools.Add(settings.type, pool);
        return pool;
    }
}