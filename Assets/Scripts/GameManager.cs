using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private int score = 0;
    [SerializeField] private float maxPlayTime = 60f; // 60 giây
    [SerializeField] private AudioClip finish;
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
            SoundManagerSO.Instance.PlaySOundFXClip(finish, transform.position, 0.75f);
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
