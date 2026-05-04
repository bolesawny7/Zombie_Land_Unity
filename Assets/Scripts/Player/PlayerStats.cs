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
        // ----- Soul Integrity (visible in inspector) -----
        [Header("Soul Integrity")]
        public float maxSoul = 100f;
        public float regenPerSecond = 6f;
        public float regenDelay = 1.5f;

        [Header("Bombs")]
        public int startingBombs = 0;

        // ----- Runtime state (Header attributes can't go on properties) -----
        public List<string> CollectedMemories { get; } = new List<string>();
        public int FragmentsCollected { get; private set; }
        public float Soul { get; private set; }
        public int BombCount { get; private set; }
        public bool IsDead => Soul <= 0f;

        public System.Action OnFragmentCollected;
        public System.Action OnSoulChanged;
        public System.Action OnBombCountChanged;

        float lastDisturbTime = -100f;

        void Awake()
        {
            Soul = maxSoul;
            BombCount = startingBombs;
        }

        void Update()
        {
            if (Time.timeScale == 0f) return;
            if (IsDead) return;

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
            if (IsDead) return;
            lastDisturbTime = Time.time;
            Soul = Mathf.Max(0f, Soul - amount);
            OnSoulChanged?.Invoke();
            if (Soul <= 0f && Managers.GameManager.Instance != null)
                Managers.GameManager.Instance.LoseGame();
        }

        public void AddBomb(int amount = 1)
        {
            BombCount += amount;
            OnBombCountChanged?.Invoke();
        }

        public bool TryConsumeBomb()
        {
            if (BombCount <= 0) return false;
            BombCount--;
            OnBombCountChanged?.Invoke();
            return true;
        }
    }
}
