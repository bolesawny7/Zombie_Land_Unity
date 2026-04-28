using UnityEngine;
using ZombieLand.Managers;
using ZombieLand.UI;

namespace ZombieLand.Items
{
    /// <summary>
    /// The "Light" the player is trying to reach. Triggers the win state once
    /// every memory fragment has been collected. If the player tries to enter
    /// early, the HUD prompts them to find the rest first.
    /// </summary>
    public class ExitPortal : MonoBehaviour
    {
        public Light glow;
        public float pulseSpeed = 1.5f;
        public float minIntensity = 1.2f;
        public float maxIntensity = 3.5f;

        void Update()
        {
            if (glow == null) return;
            float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
            glow.intensity = Mathf.Lerp(minIntensity, maxIntensity, t);
        }

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            if (GameManager.Instance == null) return;

            if (GameManager.Instance.AllFragmentsCollected)
                GameManager.Instance.WinGame();
            else if (HUDController.Instance != null)
                HUDController.Instance.ShowMessage(
                    $"You are not whole yet. {GameManager.Instance.RemainingFragments} memories remain.", 2.5f);
        }
    }
}
