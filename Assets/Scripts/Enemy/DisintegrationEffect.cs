using UnityEngine;
using System.Collections;

public class DisintegrationEffect : MonoBehaviour
{
    [SerializeField] private HealthComponent healthComponent;

    [Header("Effect Settings")]
    [SerializeField] private float duration = 1.2f;
    [SerializeField] private float spreadSpeed = 2f;
    [SerializeField] private float gravity = -1f;
    [SerializeField] private int pixelSampleStep = 2;
    [SerializeField] private Material material;

    private SpriteRenderer _sr;
    private ParticleSystem _ps;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();

        healthComponent.HealthDepleted += Disintegrate;
    }

    private void OnDisable()
    {
        healthComponent.HealthDepleted -= Disintegrate;
    }

    public void Disintegrate() => StartCoroutine(DisintegrateCoroutine());

    private IEnumerator DisintegrateCoroutine()
    {
        Texture2D tex = MakeTextureReadable(_sr.sprite.texture);
        Color[] pixels = tex.GetPixels();
        int width = tex.width;
        int height = tex.height;

        // Build particle system dynamically
        GameObject psGO = new GameObject("DisintegrationPS");
        psGO.transform.position = transform.position;
        _ps = psGO.AddComponent<ParticleSystem>();
        Destroy(psGO, duration + 1f);

        var main = _ps.main;
        main.startLifetime = duration;
        main.startSpeed = 0f;
        main.startSize = 0.05f * transform.localScale.x;
        main.maxParticles = 10000;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = gravity * 0.1f;

        var emission = _ps.emission;
        emission.enabled = false;

        var renderer = _ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = material;
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.sortingLayerName = _sr.sortingLayerName;
        renderer.sortingOrder = _sr.sortingOrder + 1;

        float ppu = _sr.sprite.pixelsPerUnit;
        Vector2 spriteSize = new Vector2(width, height) / ppu;
        Vector3 origin = transform.position - new Vector3(spriteSize.x, spriteSize.y) * 0.5f;

        for (int y = 0; y < height; y += pixelSampleStep)
        {
            for (int x = 0; x < width; x += pixelSampleStep)
            {
                Color c = pixels[y * width + x];
                if (c.a < 0.1f) continue;

                Vector3 worldPos = origin + new Vector3(
                    (x / (float)width) * spriteSize.x,
                    (y / (float)height) * spriteSize.y,
                    0f
                );

                Vector3 vel = new Vector3(
                    Random.Range(-spreadSpeed, spreadSpeed),
                    Random.Range(0f, spreadSpeed),
                    0f
                );

                var emitParams = new ParticleSystem.EmitParams();
                emitParams.position = worldPos;
                emitParams.startColor = c;
                emitParams.velocity = vel;
                emitParams.startLifetime = duration * Random.Range(0.6f, 1f);

                _ps.Emit(emitParams, 1);
            }
        }

        _sr.enabled = false;

        yield return new WaitForSeconds(duration + 0.1f);
        Destroy(gameObject);
    }
    private Texture2D MakeTextureReadable(Texture2D texture)
    {
        // Create a temporary RenderTexture
        RenderTexture tmp = RenderTexture.GetTemporary(
            texture.width,
            texture.height,
            0,
            RenderTextureFormat.Default,
            RenderTextureReadWrite.Linear
        );

        // Blit the texture to the temporary RenderTexture
        Graphics.Blit(texture, tmp);

        // Create a new Texture2D and read the pixel data from the temporary RenderTexture
        Texture2D result = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
        result.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
        result.Apply();

        // Release the temporary RenderTexture
        RenderTexture.ReleaseTemporary(tmp);

        return result;
    }
}
