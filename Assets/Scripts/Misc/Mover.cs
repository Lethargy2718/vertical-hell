using UnityEngine;

public class Mover : MonoBehaviour
{
    public enum Direction { Up, Down, Left, Right }
    public enum MoveMode { World, ScreenOffset }

    [SerializeField] private Direction _direction = Direction.Up;
    [SerializeField] private MoveMode _moveMode = MoveMode.World;
    [SerializeField] private float _speed = 1f;

    public float Speed
    {
        get => _speed;
        set => _speed = Mathf.Max(0, value);
    }

    private Camera _cam;

    private void OnEnable()
    {
        GameManager.GameStateChanged += OnGameStateChanged;
        _cam = Camera.main;
    }

    private void OnDisable()
    {
        GameManager.GameStateChanged -= OnGameStateChanged;
    }

    private void Update()
    {
        Vector3 dir = _direction switch
        {
            Direction.Up => Vector3.up,
            Direction.Down => Vector3.down,
            Direction.Left => Vector3.left,
            Direction.Right => Vector3.right,
            _ => Vector3.zero
        };

        if (_moveMode == MoveMode.World)
        {
            transform.position += _speed * Time.deltaTime * dir;
        }
        else
        {
            Vector3 screenPos = _cam.WorldToScreenPoint(transform.position);
            screenPos += _speed * 100f * Time.deltaTime * dir;
            transform.position = _cam.ScreenToWorldPoint(screenPos);
        }
    }

    private void OnGameStateChanged(GameManager.GameState gameState)
    {
        if (gameState == GameManager.GameState.Dead)
            enabled = false;
    }
}