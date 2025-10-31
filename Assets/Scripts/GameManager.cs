using UnityEngine;

public enum LevelType
{
    Normal,
    Gravity,
    Spin,

}
public enum GameState
{
    GameOver,
    Playing,
    Paused,
}

public class GameManager : MonoBehaviour
{
  
    public static GameManager Instance;
    public GameState state = 0;
    private int score = 0;
    [SerializeField] private float maxPlayTime = 30f; // 60 giây
    [SerializeField] private AudioClip lose;
    [SerializeField] private AudioClip win;
    private float currentTime;
    private bool isPlaying = false;
    private int level = 1;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        StartGame();
    }

    private void Update()
    {
        if (state== 0) return;

        currentTime -= Time.deltaTime;
        EventManager.PlayTimeElapsed(currentTime, maxPlayTime);

        if (currentTime <= 0)
        {
            currentTime = 0;
            Debug.Log("⏰ Hết thời gian!");
            GameOver();
        }
    }

    public void StartGame()
    {
        score = 0;
        currentTime = maxPlayTime;
        state = GameState.Playing;
        EventManager.ScoreChanged(score);
        EventManager.PlayTimeElapsed(currentTime, maxPlayTime);
        EventManager.NewGame();
    }

    public LevelType NextLevel()
    {
        level++;
        SoundManagerSO.Instance.PlaySOundFXClip(win, transform.position, 0.75f);
        EventManager.NewGame();
        if (level % 3 == 1)
            return LevelType.Normal;
        else if (level % 3 == 2)
            return LevelType.Gravity;
        else
            return LevelType.Spin;
    }

    public void GameOver()
    {
        state = GameState.GameOver;
        SoundManagerSO.Instance.PlaySOundFXClip(lose, transform.position,0.75f);
        EventManager.GameOver();
    }
    public void AddScore(int amount)
    {
        score += amount;
        EventManager.ScoreChanged(score);
    }

    public int GetScore() => score;
}
