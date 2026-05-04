using UnityEngine;
using UnityEngine.UI;
using ZombieLand.Player;

namespace ZombieLand.UI
{
    /// <summary>
    /// Full-screen red overlay that pulses to a high alpha when the player
    /// takes damage and decays back to zero. Lives on the same canvas as
    /// the HUD and is the immediate "I just got hit" cue.
    /// </summary>
    public class DamageFlash : MonoBehaviour
    {
        public Image overlay;
        public float maxAlpha = 0.55f;
        public float fadeSpeed = 1.2f;

        float currentAlpha;
        PlayerStats stats;

        public void Bind(PlayerStats playerStats)
        {
            stats = playerStats;
            if (stats != null) stats.OnDamaged += OnPlayerDamaged;
        }

        void OnDamaged(float amount)
        {
            // Damage 0..30 -> alpha 0.25..maxAlpha. Bigger hits -> brighter.
            float t = Mathf.Clamp01(amount / 30f);
            currentAlpha = Mathf.Max(currentAlpha, Mathf.Lerp(0.25f, maxAlpha, t));
        }

        void OnPlayerDamaged(float amount) => OnDamaged(amount);

        void Update()
        {
            if (overlay == null) return;
            if (currentAlpha > 0f)
                currentAlpha = Mathf.Max(0f, currentAlpha - fadeSpeed * Time.unscaledDeltaTime);

            Color c = overlay.color;
            c.a = currentAlpha;
            overlay.color = c;
        }

        void OnDestroy()
        {
            if (stats != null) stats.OnDamaged -= OnPlayerDamaged;
        }
    }
}
