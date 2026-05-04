using System.Collections.Generic;
using UnityEngine;

namespace ZombieLand.Player
{
    /// <summary>
    /// Tracks the player's collected memory fragments, bomb inventory, and
    /// "Soul Integrity" — the player's health. Each zombie disturbance chips
    /// Soul down; staying out of contact lets it regenerate after a short
    /// delay. When Soul reaches zero <see cref="Managers.GameManager.LoseGame"/>
    /// is called and the Lost panel takes over.
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
        // Fired whenever the player takes damage (with the amount).
        // Used by the damage-flash overlay so the player has a visceral
        // "I just got hit" cue beyond the bar tick.
        public System.Action<float> OnDamaged;

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
            OnDamaged?.Invoke(amount);
            if (Soul <= 0f && Managers.GameManager.Instance != null)
            {
                Debug.Log("[ZombieLand] Player Soul depleted -> LoseGame()");
                Managers.GameManager.Instance.LoseGame();
            }
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
