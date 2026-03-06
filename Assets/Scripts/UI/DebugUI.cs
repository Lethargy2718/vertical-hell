using UnityEngine;

public class DebugUI : MonoBehaviour
{
    [SerializeField] private GameObject debugUI;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            debugUI.SetActive(!debugUI.activeSelf);
        }
    }
}
