using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Afterimage : MonoBehaviour
{
    [SerializeField] private float spawnInterval = 0.05f;
    [SerializeField] private float fadeDuration = 0.2f;
    [SerializeField] private float afterStopDuration = 0.2f;
    [SerializeField] private Color afterimageColor = new Color(1f, 1f, 1f, 0.5f);
    [SerializeField] private Material afterimageMaterial = null;
    [SerializeField] private bool startOnSpawn = true;

    private SpriteRenderer _sr;
    private Coroutine _spawnRoutine;
    private GameObject _afterimageContainer;

    private void Awake()
    {
        _sr = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        _afterimageContainer = new GameObject(transform.parent.name + "-Afterimages");

        if (startOnSpawn)
        {
            StartAfterimages();
        }
    }

    public void StartAfterimages() => _spawnRoutine = StartCoroutine(SpawnCoroutine());
    public void StopAfterimages() => StopCoroutine(_spawnRoutine);

    public void WaitThenStopAfterImages()
    {
        StartCoroutine(WaitCoroutine());
        IEnumerator WaitCoroutine()
        {
            yield return new WaitForSeconds(afterStopDuration);
            StopAfterimages();
        }
    }

    private IEnumerator SpawnCoroutine()
    {
        while (true)
        {
            SpawnImage();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnImage()
    {
        GameObject afterImage = new GameObject("Afterimage");
        afterImage.transform.parent = _afterimageContainer.transform;
        afterImage.transform.SetPositionAndRotation(transform.position, transform.rotation);
        afterImage.transform.localScale = transform.localScale;

        SpriteRenderer sr = afterImage.AddComponent<SpriteRenderer>();
        sr.sprite = _sr.sprite;
        sr.sortingLayerID = _sr.sortingLayerID;
        sr.sortingOrder = _sr.sortingOrder - 1;
        sr.flipX = _sr.flipX;
        sr.color = afterimageColor;
        sr.material = afterimageMaterial;

        StartCoroutine(FadeOut(sr));
    }

    private IEnumerator FadeOut(SpriteRenderer sr)
    {
        float elapsed = 0f;
        Color startColor = sr.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            sr.color = new Color(startColor.r, startColor.g, startColor.b, Mathf.Lerp(startColor.a, 0f, t));
            yield return null;
        }

        Destroy(sr.gameObject);
    }

    private void OnDestroy()
    {
        if (_afterimageContainer != null)
        {
            Destroy(_afterimageContainer);
        }
    }
}