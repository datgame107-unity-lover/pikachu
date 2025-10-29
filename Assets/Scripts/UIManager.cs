using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Image timeBar;
    [SerializeField] private Gradient gradient;
    private void OnEnable()
    {
        EventManager.OnScoreChanged += UpdateScoreUI;
        EventManager.OnPlayTimeElapsed += EventManager_OnPlayTimeElapsed;
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
        scoreText.text = $"Score: {newScore}";
    }
}
