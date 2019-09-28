using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public enum GameState
{
    Prepare,
    Playing,
    Paused,
    PreGameOver,
    GameOver
}

public class GameManager : SingletonMonoBehaviour<GameManager>
{
    public static event System.Action<GameState, GameState> GameStateChanged = delegate { };

    public GameState GameState
    {
        get
        {
            return _gameState;
        }
        private set
        {
            if (value != _gameState)
            {
                GameState oldState = _gameState;
                _gameState = value;

                GameStateChanged(_gameState, oldState);
            }
        }
    }

    [SerializeField]
    private GameState _gameState = GameState.Prepare;

    public static int GameCount
    {
        get { return _gameCount; }
        private set { _gameCount = value; }
    }

    private static int _gameCount = 0;

    [Header("Gameplay Config")]
    [Range(0.0f, 1.0f)]
    public float fakeRouteRatio;
    [Range(0.0f, 1.0f)]
    public float holeRatio;

    [Range(0.1f, 4f)]
    [SerializeField]
    private float _gameSpeed = 1;
    public float gameSpeed
    {
        get { return _gameSpeed; }
        set
        {
            _gameSpeed = value;
            GameSpeedChanged();
        }
    }

    // // store the bounds.x & bounds.y value of the sprite ( in this case , the  green cube - the ground)
    // [HideInInspector] public float boundsX;
    // [HideInInspector] public float boundsY;

    void OnEnable()
    {
        PlayerController.PlayerDied += PlayerController_PlayerDied;
    }

    void OnDisable()
    {
        PlayerController.PlayerDied -= PlayerController_PlayerDied;
    }

    void Start()
    {
        // ScoreManager.Instance.Reset();
        Time.timeScale = gameSpeed;
    }

    void PlayerController_PlayerDied()
    {
        GameOver();
    }

    public void StartGame()
    {
        GameState = GameState.Playing;
    }

    public void PreGameOver()
    {
        GameState = GameState.PreGameOver;
    }

    public void GameOver()
    {
        GameState = GameState.GameOver;
    }

    public void Retry()
    {
        GameState = GameState.Playing;
    }

    void GameSpeedChanged()
    {
        Time.timeScale = gameSpeed;
    }
}