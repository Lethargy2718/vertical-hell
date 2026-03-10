using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public enum GameState { Playing, Dead }
    public static GameManager Instance { get; private set; }
    public static event Action<GameState> GameStateChanged;

    public GameState CurrentState { get; private set; }
    [HideInInspector] public int killed = 0;

    [SerializeField] private GameObject player;
    private HealthComponent playerHC;

    private bool paused = false;
    private float prevTimeScale;

    private void Awake()
    {
#if !UNITY_EDITOR
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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (paused)
                Resume();
            else
                Pause();
        }
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

    private void Pause()
    {
        prevTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        paused = true;
    }

    public void Resume()
    {
        Time.timeScale = prevTimeScale;
        paused = false;
    }
}
