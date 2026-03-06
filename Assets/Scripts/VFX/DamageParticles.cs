using System.Collections;
using UnityEngine;
public class DamageParticles : MonoBehaviour
{
    [Header("Particles")]
    [SerializeField] private Color particleColor = Color.black;
    [SerializeField] private int particleCount = 12;
    [SerializeField] private float spreadSpeed = 3f;
    [SerializeField] private float lifetime = 0.5f;
    [SerializeField] private float particleSize = 0.08f;
    [SerializeField] private float gravity = -2f;

    private HealthComponent _healthComponent;

    private void Awake()
    {
        _healthComponent = GetComponentInParent<HealthComponent>();
    }

    private void OnEnable()
    {
        _healthComponent.DamageTaken += OnDamageTaken;
    }

    private void OnDisable()
    {
        _healthComponent.DamageTaken -= OnDamageTaken;
    }

    private void OnDamageTaken(float _, Vector2 attackDirection)
    {
        SpawnParticles(attackDirection);
    }

    private void SpawnParticles(Vector2 direction)
    {
        for (int i = 0; i < particleCount; i++)
        {
            SpawnPixel(direction);
        }
    }

    private void SpawnPixel(Vector2 direction)
    {
        GameObject pixel = new GameObject("DamageParticle");
        pixel.transform.position = transform.position;

        SpriteRenderer sr = pixel.AddComponent<SpriteRenderer>();
        sr.sprite = GetSquareSprite();
        sr.color = particleColor;
        sr.sortingOrder = 10;

        Vector2 velocity = direction * spreadSpeed + new Vector2(
            Random.Range(-spreadSpeed, spreadSpeed) * 0.5f,
            Random.Range(-spreadSpeed, spreadSpeed) * 0.5f
        );

        StartCoroutine(AnimatePixel(pixel, sr, velocity));
    }

    private IEnumerator AnimatePixel(GameObject pixel, SpriteRenderer sr, Vector2 velocity)
    {
        float elapsed = 0f;
        Vector3 pos = pixel.transform.position;
        Color startColor = sr.color;

        while (elapsed < lifetime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / lifetime;

            velocity.y += gravity * Time.deltaTime;
            pos += (Vector3)velocity * Time.deltaTime;
            pixel.transform.position = pos;
            pixel.transform.localScale = Vector3.one * particleSize * (1f - t * 0.5f);

            sr.color = new Color(startColor.r, startColor.g, startColor.b, Mathf.Lerp(1f, 0f, t));

            yield return null;
        }

        Destroy(pixel);
    }

    private Sprite GetSquareSprite()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.filterMode = FilterMode.Point;
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 16f);
    }
}