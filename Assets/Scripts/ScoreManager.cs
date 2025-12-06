using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    // backing field (kept private) and public read-only property
    int score = 0;
    public int Points { get { return score; } }

    void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Add (or subtract) points. Keeps Points >= 0.
    /// Also updates the UI if UIManager exists.
    /// </summary>
    public void AddPoints(int pts)
    {
        score += pts;
        if (score < 0) score = 0;
        UIManager.Instance?.UpdateScore(score);
    }

    /// <summary>
    /// Set the points directly (useful for resetting the run).
    /// </summary>
    public void SetPoints(int value)
    {
        score = Mathf.Max(0, value);
        UIManager.Instance?.UpdateScore(score);
    }

    /// <summary>
    /// Reset score to 0 and update UI.
    /// </summary>
    public void ResetScore()
    {
        score = 0;
        UIManager.Instance?.UpdateScore(score);
    }
}
