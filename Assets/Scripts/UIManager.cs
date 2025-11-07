using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("References")]
    public Image healthBar;       // Your HealthBar image
    public TMP_Text scoreText;
    public TMP_Text waveText;
    public TMP_Text blasterStatusText;

    void Awake()
    {
        Instance = this;
    }

    public void UpdateHealth(float current, float max)
    {
        if (healthBar)
            healthBar.fillAmount = current / max;
    }

    public void UpdateScore(int score)
    {
        if (scoreText)
            scoreText.text = "Score: " + score;
    }

    public void UpdateWave(int wave)
    {
        if (waveText)
            waveText.text = "Wave: " + wave;
    }
    
    public void UpdateBlasterStatus(int current, int max)
    {
        if (blasterStatusText)
            blasterStatusText.text = $"{current} / {max}";
    }

}
