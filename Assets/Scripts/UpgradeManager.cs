using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    [Header("UI")]
    public GameObject upgradeMenuUI;
    public TMP_Text pointsText;
    public GameObject crosshair; // optional: assign your crosshair UI image here
    public int costPerTier = 100;
    public AudioClip upgradeConfirmSound;
    public AudioSource uiAudioSource;   
    // tiers / state
    public int turboTier = 0;
    public int platingTier = 0;
    public int regulatorTier = 0;
    public int rapidPulseTier = 0;
    public int amplifierTier = 0;
    public int coolingVentsBought = 0;
    public int multiShotTier = 0;

    // Exposed property CameraOrbit and other scripts can check
    public bool IsOpen { get; private set; } = false;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        // keep points text in sync while menu open (safe null checks)
        if (upgradeMenuUI != null && upgradeMenuUI.activeSelf && pointsText != null)
            pointsText.text = $"Upgrade Points: {ScoreManager.Instance?.Points ?? 0}";
    }

    public void OpenMenu()
    {
        if (upgradeMenuUI == null) return;

        // Activate UI first so it's visible before we pause
        upgradeMenuUI.SetActive(true);

        // Show cursor and unlock it so player can interact with UI
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Hide crosshair while menu is open
        if (crosshair != null) crosshair.SetActive(false);

        // Select first button for keyboard/gamepad nav if available
        if (EventSystem.current != null)
        {
            var firstButton = upgradeMenuUI.GetComponentInChildren<UnityEngine.UI.Button>();
            if (firstButton != null)
                EventSystem.current.SetSelectedGameObject(firstButton.gameObject);
            else
                EventSystem.current.SetSelectedGameObject(null);
        }

        // Pause game time
        Time.timeScale = 0f;
        IsOpen = true;
    }

    public void CloseMenu()
    {
        if (upgradeMenuUI == null) return;

        // Deactivate UI
        upgradeMenuUI.SetActive(false);

        // Restore cursor to gameplay state: hidden + locked
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Show crosshair again
        if (crosshair != null) crosshair.SetActive(true);

        // Resume time
        Time.timeScale = 1f;
        IsOpen = false;

        // Clear EventSystem selection
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    bool SpendPoints(int cost)
    {
        if (ScoreManager.Instance == null || ScoreManager.Instance.Points < cost) return false;
        ScoreManager.Instance.AddPoints(-cost);
        uiAudioSource.PlayOneShot(upgradeConfirmSound);
        return true;
    }

    // ------- Player upgrades -------
    public void BuyTurboServos()
    {
        if (turboTier >= 5) return;
        if (!SpendPoints(costPerTier)) return;
        turboTier++;
        PlayerMovement pm = FindObjectOfType<PlayerMovement>();
        if (pm != null) pm.moveSpeed *= 1.10f;
    }

    public void BuyReinforcedPlating()
    {
        if (platingTier >= 5) return;
        if (!SpendPoints(costPerTier)) return;
        platingTier++;
        HealthSystem hs = FindObjectOfType<HealthSystem>();
        if (hs != null)
        {
            hs.maxHealth = Mathf.CeilToInt(hs.maxHealth * 1.20f);
            hs.Heal(hs.maxHealth * 0.20f);
        }
    }

    public void BuyEnergyRegulator()
    {
        if (regulatorTier >= 5) return;
        if (!SpendPoints(costPerTier)) return;
        regulatorTier++;

        HealthSystem hs = FindObjectOfType<HealthSystem>();
        if (hs != null)
        {
            hs.ApplyRegenTier(1); // increases regen speed by 25% and shortens delay proportionally
        }
    }

    // ------- Blaster upgrades -------
    public void BuyRapidPulse()
    {
        if (rapidPulseTier >= 5) return;
        if (!SpendPoints(costPerTier)) return;
        rapidPulseTier++;
        BlasterController bc = FindObjectOfType<BlasterController>();
        if (bc != null) bc.fireRate *= 0.8f;
    }

    public void BuyAmplifierCoil()
    {
        if (amplifierTier >= 5) return;
        if (!SpendPoints(costPerTier)) return;
        amplifierTier++;
        BlasterController bc = FindObjectOfType<BlasterController>();
        if (bc != null) bc.projectileDamage = Mathf.CeilToInt(bc.projectileDamage * 1.2f);
    }

    public void BuyCoolingVents()
    {
        if (coolingVentsBought >= 1) return;
        if (!SpendPoints(costPerTier)) return;
        coolingVentsBought = 1;
        BlasterController bc = FindObjectOfType<BlasterController>();
        if (bc != null) bc.reloadTime *= 0.5f;
    }

    public void BuyMultiShot()
    {
        if (multiShotTier >= 3) return;
        if (!SpendPoints(costPerTier)) return;
        multiShotTier++;
        BlasterController bc = FindObjectOfType<BlasterController>();
        if (bc != null) bc.multiShotCount += 1;
    }

    // Called by Continue button in the UI
    public void ContinueFromMenu()
    {
        CloseMenu();
        var wm = FindObjectOfType<WaveManager>();
        if (wm != null) wm.ContinueToNextWave();
    }
}
