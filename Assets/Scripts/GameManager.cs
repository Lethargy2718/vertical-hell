using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public enum GameState { Playing, Dead, Paused }
    public static GameManager Instance { get; private set; }
    public static event Action<GameState> GameStateChanged;

    public GameState CurrentState { get; private set; }
    public int killed = 0;

    [SerializeField] private GameObject player;
    private HealthComponent playerHC;

    private void Awake()
    {
#if UNITY_EDITOR
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
#endif

        Instance = this;

        if (player == null)
        {
            player = FindFirstObjectByType<PlayerStateDriver>().gameObject;
        }
        player.TryGetComponent(out playerHC);
    }

    private void OnEnable()
    {
        playerHC.HealthDepleted += EndGame;
    }

    private void OnDisable()
    {
        playerHC.HealthDepleted -= EndGame;
    }

    private void EndGame()
    {
        ChangeState(GameState.Dead);
    }

    public void ChangeState(GameState newState)
    {
        CurrentState = newState;
        GameStateChanged?.Invoke(newState);
    }
}
