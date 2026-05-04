using System.Collections.Generic;
using UnityEngine;

namespace ZombieLand.Player
{
    /// <summary>
    /// Tracks the player's collected memory fragments AND the player's
    /// "Soul Integrity" — a forgiving health stat that drops when a zombie
    /// disturbs you and slowly regenerates over time.
    ///
    /// To preserve the project's "you are already a ghost, you cannot die"
    /// theme, Soul Integrity is clamped at <see cref="minSoul"/> &gt; 0 —
    /// it can be visibly drained and recovered, but it can't kill you.
    /// </summary>
    public class PlayerStats : MonoBehaviour
    {
        [Header("Memory fragments")]
        public List<string> CollectedMemories { get; } = new List<string>();
        public int FragmentsCollected { get; private set; }
        public System.Action OnFragmentCollected;

        [Header("Soul Integrity")]
        public float maxSoul = 100f;
        public float minSoul = 5f;
        public float regenPerSecond = 6f;
        public float regenDelay = 1.5f;

        public float Soul { get; private set; }
        public System.Action OnSoulChanged;

        float lastDisturbTime = -100f;

        void Awake()
        {
            Soul = maxSoul;
        }

        void Update()
        {
            if (Time.timeScale == 0f) return;

            // Regenerate Soul, but only after a brief cooldown since the
            // last disturbance — gives the bar a "recover" feel.
            if (Time.time - lastDisturbTime >= regenDelay && Soul < maxSoul)
            {
                Soul = Mathf.Min(maxSoul, Soul + regenPerSecond * Time.deltaTime);
                OnSoulChanged?.Invoke();
            }
        }

        public void CollectFragment(string memoryText)
        {
            FragmentsCollected++;
            if (!string.IsNullOrEmpty(memoryText))
                CollectedMemories.Add(memoryText);

            OnFragmentCollected?.Invoke();
        }

        public void DisturbSoul(float amount)
        {
            lastDisturbTime = Time.time;
            // Floor at minSoul so the player never reaches zero / dies.
            Soul = Mathf.Max(minSoul, Soul - amount);
            OnSoulChanged?.Invoke();
        }
    }
}
