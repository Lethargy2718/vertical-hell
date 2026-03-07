using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class ButtonTextColorTransition : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Target Text")]
    public TextMeshProUGUI targetText;

    [Header("Colors")]
    public Color normalColor = Color.white;
    public Color highlightColor = Color.yellow;
    public Color pressedColor = Color.gray;
    public Color disabledColor = new Color(1, 1, 1, 0.5f);

    [Header("Transition")]
    [Range(0f, 5f)]
    public float fadeDuration = 0.1f;

    private Coroutine _fadeCoroutine;
    private Button _button;

    void Start()
    {
        _button = GetComponent<Button>();
        if (targetText) targetText.color = normalColor;
    }

    public void OnPointerEnter(PointerEventData e) => FadeTo(highlightColor);
    public void OnPointerExit(PointerEventData e) => FadeTo(normalColor);
    public void OnPointerDown(PointerEventData e) => FadeTo(pressedColor);
    public void OnPointerUp(PointerEventData e) => FadeTo(highlightColor);

    void FadeTo(Color target)
    {
        if (_button && !_button.interactable) target = disabledColor;
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(FadeCoroutine(target));
    }

    System.Collections.IEnumerator FadeCoroutine(Color target)
    {
        Color start = targetText.color;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            targetText.color = Color.Lerp(start, target, elapsed / fadeDuration);
            yield return null;
        }
        targetText.color = target;
    }
}