using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{   
    
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Image timeBar;
    [SerializeField] private Gradient gradient;
    [SerializeField] private Image gameOver;
    [SerializeField] private TextMeshProUGUI levelText;
    private void OnEnable()
    {
        EventManager.OnScoreChanged += EventManager_OnScoreChanged; ;
        EventManager.OnPlayTimeElapsed += EventManager_OnPlayTimeElapsed;
        EventManager.OnGameOver += EventManager_OnGameOver;
        EventManager.OnNewGame += EventManager_OnNewGame;
        EventManager.OnNextLevel += EventManager_OnNextLevel;
    }

    private void EventManager_OnNextLevel(int level)
    {
        levelText.text = level.ToString();

    }

    private void EventManager_OnScoreChanged(int newScore)
    {
        scoreText.text =newScore.ToString();

    }

    private void EventManager_OnNewGame()
    {
        gameOver.gameObject.SetActive(false);
    }

    private void EventManager_OnGameOver()
    {
        gameOver.gameObject.SetActive(true);

    }
    private void EventManager_OnPlayTimeElapsed(float currentTime, float maxTime)
    {
        float ratio = Mathf.Clamp01(currentTime / maxTime);
        timeBar.fillAmount = ratio;
        timeBar.color = gradient.Evaluate(ratio);
    } 

    private void OnDisable()
    {
        EventManager.OnScoreChanged -= UpdateScoreUI;
    }

    private void UpdateScoreUI(int newScore)
    {
    }
}
