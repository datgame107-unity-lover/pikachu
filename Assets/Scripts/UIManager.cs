using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Listens to <see cref="EventManager"/> events and updates all HUD elements:
/// score, timer bar, level label, and the game-over overlay.
/// </summary>
public class UIManager : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector fields
    // -------------------------------------------------------------------------

    [Header("HUD")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI levelText;

    [Header("Timer")]
    [SerializeField] private Image timerBar;
    [SerializeField] private Gradient timerGradient;

    [Header("Overlays")]
    [SerializeField] private GameObject gameOverOverlay;
    [SerializeField] private Button newGameButton;

    // -------------------------------------------------------------------------
    // Unity messages
    // -------------------------------------------------------------------------

    private void OnEnable()
    {
        EventManager.OnScoreChanged += HandleScoreChanged;
        EventManager.OnPlayTimeElapsed += HandlePlayTimeElapsed;
        EventManager.OnGameOver += HandleGameOver;
        EventManager.OnNewGame += HandleNewGame;
        EventManager.OnNextLevel += HandleNextLevel;

        newGameButton?.onClick.AddListener(() =>
        GameManager.Instance.StartGame()


        );
    }

    private void OnDisable()
    {
        EventManager.OnScoreChanged -= HandleScoreChanged;
        EventManager.OnPlayTimeElapsed -= HandlePlayTimeElapsed;
        EventManager.OnGameOver -= HandleGameOver;
        EventManager.OnNewGame -= HandleNewGame;
        EventManager.OnNextLevel -= HandleNextLevel;
        newGameButton.onClick.RemoveAllListeners();
    }

    // -------------------------------------------------------------------------
    // Event handlers
    // -------------------------------------------------------------------------

    private void HandleScoreChanged(int newScore)
    {
        if (scoreText != null)
            scoreText.text = newScore.ToString();
    }

    private void HandlePlayTimeElapsed(float currentTime, float maxTime)
    {
        if (timerBar == null) return;

        float ratio = Mathf.Clamp01(currentTime / maxTime);
        timerBar.fillAmount = ratio;
        timerBar.color = timerGradient.Evaluate(ratio);
    }

    private void HandleGameOver()
    {
        if (gameOverOverlay != null)
            gameOverOverlay.SetActive(true);
    }

    private void HandleNewGame()
    {
        if (gameOverOverlay != null)
            gameOverOverlay.SetActive(false);
    }

    private void HandleNextLevel(int level)
    {
        if (levelText != null)
            levelText.text = level.ToString();
    }
}