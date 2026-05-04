using UnityEngine;
using UnityEngine.UI;
using ZombieLand.Managers;
using ZombieLand.Player;

namespace ZombieLand.UI
{
    /// <summary>
    /// Owns the in-game HUD: fragment counter, flashlight battery bar,
    /// sprint stamina bar, soul-integrity (health) bar, and a transient
    /// message line used for collected-memory text and zombie disturbance hints.
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        public static HUDController Instance { get; private set; }

        public Text fragmentText;
        public Text messageText;
        public Text bombText;
        public Image batteryFill;
        public Image staminaFill;
        public Image soulFill;

        PlayerStats stats;
        PlayerFlashlight flashlight;
        PlayerController controller;

        float messageTimer;

        void Awake() { Instance = this; }

        public void Bind(PlayerStats playerStats, PlayerFlashlight playerFlashlight, PlayerController playerController)
        {
            stats = playerStats;
            flashlight = playerFlashlight;
            controller = playerController;

            if (stats != null)
            {
                stats.OnFragmentCollected += RefreshFragmentCount;
                stats.OnBombCountChanged += RefreshBombCount;
            }
            RefreshFragmentCount();
            RefreshBombCount();
        }

        void Update()
        {
            if (flashlight != null && batteryFill != null)
                batteryFill.fillAmount = flashlight.Battery / flashlight.maxBattery;

            if (controller != null && staminaFill != null)
            {
                staminaFill.fillAmount = controller.Stamina / controller.maxStamina;
                Color baseColor = new Color(0.6f, 0.85f, 1f);
                Color sprintColor = new Color(0.85f, 1f, 0.7f);
                staminaFill.color = controller.IsSprinting ? sprintColor : baseColor;
            }

            if (stats != null && soulFill != null)
            {
                soulFill.fillAmount = stats.Soul / stats.maxSoul;
                // Tint redder when low — adds tension without screen filters.
                float t = Mathf.Clamp01(stats.Soul / stats.maxSoul);
                soulFill.color = Color.Lerp(new Color(1f, 0.25f, 0.3f), new Color(1f, 0.55f, 0.55f), t);
            }

            if (messageTimer > 0f)
            {
                messageTimer -= Time.unscaledDeltaTime;
                if (messageTimer <= 0f && messageText != null) messageText.text = "";
            }
        }

        public void RefreshFragmentCount()
        {
            if (fragmentText == null || GameManager.Instance == null) return;
            int collected = stats != null ? stats.FragmentsCollected : GameManager.Instance.collectedFragments;
            fragmentText.text = $"Memories: {collected} / {GameManager.Instance.totalFragments}";
        }

        public void RefreshBombCount()
        {
            if (bombText == null || stats == null) return;
            bombText.text = $"Bombs: {stats.BombCount}";
        }

        public void ShowMessage(string message, float duration)
        {
            if (messageText == null) return;
            messageText.text = message;
            messageTimer = duration;
        }
    }
}
