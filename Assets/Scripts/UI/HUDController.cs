using UnityEngine;
using UnityEngine.UI;
using ZombieLand.Managers;
using ZombieLand.Player;

namespace ZombieLand.UI
{
    /// <summary>
    /// Owns the in-game HUD: fragment counter, battery bar, and a transient
    /// message line used for collected-memory text and zombie disturbance hints.
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        public static HUDController Instance { get; private set; }

        public Text fragmentText;
        public Text messageText;
        public Image batteryFill;

        PlayerStats stats;
        PlayerFlashlight flashlight;

        float messageTimer;

        void Awake() { Instance = this; }

        public void Bind(PlayerStats playerStats, PlayerFlashlight playerFlashlight)
        {
            stats = playerStats;
            flashlight = playerFlashlight;

            if (stats != null) stats.OnFragmentCollected += RefreshFragmentCount;
            RefreshFragmentCount();
        }

        void Update()
        {
            if (flashlight != null && batteryFill != null)
                batteryFill.fillAmount = flashlight.Battery / flashlight.maxBattery;

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

        public void ShowMessage(string message, float duration)
        {
            if (messageText == null) return;
            messageText.text = message;
            messageTimer = duration;
        }
    }
}
