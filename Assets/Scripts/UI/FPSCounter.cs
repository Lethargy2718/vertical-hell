using UnityEngine;
using TMPro;
using System.Collections;

public class FPSCounter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private float fpsUpdateInterval = 0.1f;

    private void Start()
    {
        StartCoroutine(UpdateFPSCoroutine());
    }

    private IEnumerator UpdateFPSCoroutine()
    {
        while (true)
        {
            text.text = $"{(int)(1f / Time.deltaTime)} FPS";
            yield return new WaitForSecondsRealtime(fpsUpdateInterval);
        }
    }



}