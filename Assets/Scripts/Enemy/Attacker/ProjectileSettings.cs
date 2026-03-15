using UnityEngine;

[CreateAssetMenu(menuName = "Flyweight/Projectile Settings")]
public class ProjectileSettings : FlyweightSettings
{
    public float lifetime = 5f;
    public float damage = 10f;

    public override Flyweight Create()
    {
        var go = Instantiate(prefab);
        go.SetActive(false);
        go.name = prefab.name;

        if (!go.TryGetComponent<Projectile>(out var flyweight)) 
            flyweight = go.AddComponent<Projectile>();

        flyweight.settings = this;
        return flyweight;
    }
}