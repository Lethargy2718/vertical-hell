using UnityEngine;

public class DebugUI : MonoBehaviour
{
    [SerializeField] private GameObject debugUI;
    [SerializeField] private bool startEnabled = false;
    private bool debugUIEnabled;

    private void Start()
    {
        SetDebugUIEnabled(startEnabled);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            SetDebugUIEnabled(!debugUIEnabled);
        }
    }

    private void SetDebugUIEnabled(bool enabled) 
    {
        debugUIEnabled = enabled;
        debugUI.SetActive(enabled);
    }
}
