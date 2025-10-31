using System;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    // Sự kiện cộng điểm
    public static event Action<int> OnScoreChanged;

    public static event Action OnGameOver;
    public static event Action OnNewGame;
    public static event Action<float,float> OnPlayTimeElapsed;
    public static event Action<int> OnNextLevel;
    // Gọi khi điểm thay đổi
    public static void ScoreChanged(int newScore)
    {
        OnScoreChanged?.Invoke(newScore);
    }
    public static void GameOver()
    {
        OnGameOver?.Invoke();
    }
    public static void NewGame()
    {
        OnNewGame?.Invoke();
    }
    public static void NextLevel(int level)
    {
        OnNextLevel?.Invoke(level);
    }
    public static void PlayTimeElapsed(float elapsedTime, float totalTime)
    {
        OnPlayTimeElapsed?.Invoke(elapsedTime, totalTime);
    }
}
