using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private int score = 0;
    [SerializeField] private float maxPlayTime = 60f; // 60 giây
    private float currentTime;
    private bool isPlaying = false;

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
        if (!isPlaying) return;

        currentTime -= Time.deltaTime;
        EventManager.PlayTimeElapsed(currentTime, maxPlayTime);

        if (currentTime <= 0)
        {
            currentTime = 0;
            isPlaying = false;
            Debug.Log("⏰ Hết thời gian!");
        }
    }

    public void StartGame()
    {
        score = 0;
        currentTime = maxPlayTime;
        isPlaying = true;
        EventManager.ScoreChanged(score);
        EventManager.PlayTimeElapsed(currentTime, maxPlayTime);
    }

    public void AddScore(int amount)
    {
        score += amount;
        EventManager.ScoreChanged(score);
    }

    public int GetScore() => score;
}
