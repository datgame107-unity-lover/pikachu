using System;

/// <summary>
/// Centralised static event bus. Components raise events here; listeners
/// subscribe/unsubscribe without needing direct references to each other.
/// </summary>
public static class EventManager
{
    // -------------------------------------------------------------------------
    // Events
    // -------------------------------------------------------------------------

    /// <summary>Fired whenever the player's score changes. Passes the new total.</summary>
    public static event Action<int> OnScoreChanged;

    /// <summary>Fired when the timer reaches zero or the game ends.</summary>
    public static event Action OnGameOver;

    /// <summary>Fired when a new game session begins.</summary>
    public static event Action OnNewGame;

    /// <summary>
    /// Fired each frame the game timer ticks.
    /// Parameters: (elapsed seconds, total seconds).
    /// </summary>
    public static event Action<float, float> OnPlayTimeElapsed;

    /// <summary>Fired when the player advances to a new level. Passes the new level number.</summary>
    public static event Action<int> OnNextLevel;

    // -------------------------------------------------------------------------
    // Raise helpers  (prefer calling these over invoking events directly)
    // -------------------------------------------------------------------------

    public static void RaiseScoreChanged(int newScore) => OnScoreChanged?.Invoke(newScore);
    public static void RaiseGameOver() => OnGameOver?.Invoke();
    public static void RaiseNewGame() => OnNewGame?.Invoke();
    public static void RaiseNextLevel(int level) => OnNextLevel?.Invoke(level);
    public static void RaisePlayTimeElapsed(float elapsed, float total)
                                                                 => OnPlayTimeElapsed?.Invoke(elapsed, total);
}