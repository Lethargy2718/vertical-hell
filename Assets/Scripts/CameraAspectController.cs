using UnityEngine;

[ExecuteAlways]
public class CameraAspectController : MonoBehaviour
{
    [Tooltip("Vertical world units the camera must always show.")]
    public float targetVerticalUnits = 11.25f;

    [Tooltip("Minimum horizontal world units the camera must show.")]
    public float targetHorizontalUnits = 20f;

    private Camera cam;
    private int lastWidth;
    private int lastHeight;

    private void Start()
    {
        cam = GetComponent<Camera>();
        ApplyAspect();
    }

    private void Update()
    {
        // Check for resolution changes
        if (Screen.width != lastWidth || Screen.height != lastHeight)
        {
            ApplyAspect();
            lastWidth = Screen.width;
            lastHeight = Screen.height;
        }
    }

    private void ApplyAspect()
    {
        float targetAspect = targetHorizontalUnits / targetVerticalUnits;
        float screenAspect = (float)Screen.width / Screen.height;

        if (screenAspect >= targetAspect)
        {
            // Screen is wider or equal: use full screen, let horizontal grow beyond 20 units
            cam.aspect = screenAspect;
            cam.rect = new Rect(0, 0, 1, 1);
        }
        else
        {
            // Screen is narrower: force horizontal to exactly 20 units, letterbox vertically
            cam.aspect = targetAspect;
            float viewportHeight = screenAspect / targetAspect; // < 1
            float viewportY = (1f - viewportHeight) / 2f;
            cam.rect = new Rect(0, viewportY, 1, viewportHeight);
        }
    }
}