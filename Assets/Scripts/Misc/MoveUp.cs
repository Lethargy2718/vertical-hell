using UnityEngine;

public class MoveUp : MonoBehaviour
{
    [SerializeField] private float speed = 1.0f;

    private void OnEnable()
    {
        GameManager.GameStateChanged += OnGameStateChanged;
    }

    private void OnDisable()
    {
        GameManager.GameStateChanged -= OnGameStateChanged;
    }

    private void Update()
    {
        transform.Translate(speed * Time.deltaTime * Vector2.up);
    }

    private void OnGameStateChanged(GameManager.GameState gameState)
    {
        if (gameState == GameManager.GameState.Dead)
        {
            enabled = false;
        }
    }
}
