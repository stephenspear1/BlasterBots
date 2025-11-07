using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    int score = 0;

    void Awake() => Instance = this;

    public void AddPoints(int pts)
    {
        score += pts;
        UIManager.Instance.UpdateScore(score);
    }

    public void ResetScore()
    {
        score = 0;
        UIManager.Instance.UpdateScore(score);
    }
}