using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Lightweight UI helper for health, score, wave, blaster ammo, boss warning and victory panel.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("HUD")]
    public Image healthBar;          // set to Filled Image
    public TMP_Text scoreText;
    public TMP_Text waveText;
    public TMP_Text blasterStatusText;

    [Header("Boss")]
    public TMP_Text bossWarningText;

    [Header("Panels")]
    public GameObject victoryPanel;  // assign your victory panel here

    void Awake() { Instance = this; }

    public void UpdateHealth(float current, float max)
    {
        if (healthBar) healthBar.fillAmount = Mathf.Clamp01(current / max);
    }

    public void UpdateScore(int score)
    {
        if (scoreText) scoreText.text = $"Score: {score}";
    }

    public void UpdateWave(int wave)
    {
        if (waveText) waveText.text = $"Wave: {wave}";
    }

    public void UpdateBlasterStatus(int current, int max)
    {
        if (blasterStatusText) blasterStatusText.text = $"{current} / {max}";
    }

    public void ShowBossWarning()
    {
        if (bossWarningText == null) return;
        StopAllCoroutines();
        StartCoroutine(BossWarningCoroutine());
    }

    IEnumerator BossWarningCoroutine()
    {
        bossWarningText.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(2f);
        bossWarningText.gameObject.SetActive(false);
    }

    public void ShowVictoryScreen()
    {
        if (victoryPanel != null) victoryPanel.SetActive(true);
        Time.timeScale = 0f;
    }
}
