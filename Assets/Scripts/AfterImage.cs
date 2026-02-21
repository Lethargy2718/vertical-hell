using System.Collections;
using UnityEngine;

public class Afterimage : MonoBehaviour
{
    [SerializeField] private float spawnInterval = 0.05f;
    [SerializeField] private float fadeDuration = 0.2f;
    [SerializeField] private float afterDashTrailDuration = 0.2f;
    [SerializeField] private Color afterimageColor = new Color(1f, 1f, 1f, 0.5f);
    [SerializeField] private Material afterimageMaterial = null;

    private SpriteRenderer _sr;
    private PlayerController _playerController;
    private Coroutine _spawnRoutine;

    private void Awake()
    {
        _sr = GetComponentInChildren<SpriteRenderer>();
        _playerController = GetComponent<PlayerController>();
    }

    private void OnEnable()
    {
        _playerController.Dashed += StartAfterimages;
        _playerController.DashEnded += WaitThenStopAfterImages;
    }

    private void OnDisable()
    {
        _playerController.Dashed -= StartAfterimages;
        _playerController.DashEnded -= WaitThenStopAfterImages;
    }

    public void StartAfterimages() => _spawnRoutine = StartCoroutine(SpawnCoroutine());
    public void StopAfterimages() => StopCoroutine(_spawnRoutine);

    private void WaitThenStopAfterImages()
    {
        StartCoroutine(WaitCoroutine());
        IEnumerator WaitCoroutine()
        {
            yield return new WaitForSeconds(afterDashTrailDuration);
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
        GameObject afterImage = new GameObject();
        afterImage.transform.position = transform.position;
        afterImage.transform.rotation = transform.rotation;
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
}