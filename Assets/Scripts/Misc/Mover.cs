using UnityEngine;

[DefaultExecutionOrder(1000)]
public class Mover : MonoBehaviour
{
    public enum Direction { Up, Down, Left, Right }

    [SerializeField] private Direction direction = Direction.Up;
    [SerializeField] private float speed = 1f;

    public float Speed
    {
        get => speed;
        set => speed = Mathf.Max(0, value);
    }

    private Vector2 MoveDirection => direction switch
    {
        Direction.Up => Vector2.up,
        Direction.Down => Vector2.down,
        Direction.Left => Vector2.left,
        Direction.Right => Vector2.right,
        _ => Vector2.zero
    };

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
        transform.position += speed * Time.deltaTime * (Vector3)MoveDirection;
    }

    private void OnGameStateChanged(GameManager.GameState gameState)
    {
        if (gameState == GameManager.GameState.Dead)
            enabled = false;
    }
}