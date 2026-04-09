using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

/// <summary>
/// Manages game state, scoring, and the countdown timer.
/// Persists across scenes via <see cref="DontDestroyOnLoad"/>.
/// </summary>
 public enum GameState
{
    Playing,
    Paused,
    GameOver,
}
public enum LevelType
{
    Normal,
    Gravity,
    Spin

}
public class GameManager : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Singleton
    // -------------------------------------------------------------------------

    public static GameManager Instance { get; private set; }

    // -------------------------------------------------------------------------
    // Inspector fields
    // -------------------------------------------------------------------------

    [Header("Gameplay")]
    [SerializeField, Min(1f)] private float maxPlayTime = 60f;

    [Header("Audio")]
    [SerializeField] private AudioClip loseClip;
    [SerializeField] private AudioClip winClip;
    [SerializeField] private AudioSource backgroundMusicSource;

    // -------------------------------------------------------------------------
    // Public read-only state
    // -------------------------------------------------------------------------

    public GameState State { get; private set; }
    public float Volume { get; private set; } = 0.45f;

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private int _score;
    private float _remainingTime;
    private int _level = 1;

    // -------------------------------------------------------------------------
    // Unity messages
    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start() => StartGame();

    private void Update()
    {
        if (State != GameState.Playing) return;

        _remainingTime -= Time.deltaTime;
        EventManager.RaisePlayTimeElapsed(_remainingTime, maxPlayTime);

        if (_remainingTime <= 0f)
        {
            _remainingTime = 0f;
            Debug.Log("[GameManager] Time's up!");
            TriggerGameOver();
        }
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>Resets score, timer, and state then broadcasts a new-game event.</summary>
    public void StartGame()
    {
        _score = 0;
        _remainingTime = maxPlayTime;
        State = GameState.Playing;

        EventManager.RaiseScoreChanged(_score);
        EventManager.RaisePlayTimeElapsed(_remainingTime, maxPlayTime);
        EventManager.RaiseNewGame();
    }

    /// <summary>
    /// Advances to the next level, adjusts remaining time, and returns the
    /// <see cref="LevelType"/> for the new level.
    /// </summary>
    public LevelType AdvanceToNextLevel()
    {
        _level++;
        _remainingTime = Mathf.Clamp(maxPlayTime - 10f * _level, maxPlayTime * 0.5f, maxPlayTime);
        State = GameState.Playing;

        SoundManagerSO.Instance.PlaySoundFX(winClip, transform.position, Volume);
        EventManager.RaisePlayTimeElapsed(_remainingTime, maxPlayTime);
        EventManager.RaiseNextLevel(_level);

        return LevelTypeForLevel(_level);
    }

    /// <summary>Ends the game session.</summary>
    public void TriggerGameOver()
    {
        State = GameState.GameOver;
        SoundManagerSO.Instance.PlaySoundFX(loseClip, transform.position, Volume);
        EventManager.RaiseGameOver();
    }

    /// <summary>Increments the player's score by <paramref name="amount"/>.</summary>
    public void AddScore(int amount)
    {
        _score += amount;
        EventManager.RaiseScoreChanged(_score);
    }

    public int GetScore() => _score;

    /// <summary>Toggles background music and sound effects between muted and normal volume.</summary>
    public void ToggleSound()
    {
        Volume = Volume > 0f ? 0f : 0.45f;

        if (backgroundMusicSource != null)
            backgroundMusicSource.volume = Volume;
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private static LevelType LevelTypeForLevel(int level)
    {
        return (level % 3) switch
        {
            1 => LevelType.Normal,
            2 => LevelType.Gravity,
            _ => LevelType.Spin,
        };
    }
}