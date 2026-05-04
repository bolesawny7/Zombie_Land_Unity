using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ZombieLand.Managers
{
    /// <summary>
    /// Central, lightweight game-state machine. Owns the count of memory
    /// fragments in the level and the play/pause/win flow.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public enum State { Menu, Playing, Paused, Won, Lost }

        public static GameManager Instance { get; private set; }

        public int totalFragments = 5;
        public int collectedFragments = 0;
        public List<string> rememberedMemories = new List<string>();

        public State CurrentState { get; private set; } = State.Menu;
        public System.Action<State> OnStateChanged;

        public bool AllFragmentsCollected => collectedFragments >= totalFragments;
        public int RemainingFragments => Mathf.Max(0, totalFragments - collectedFragments);

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Pause the simulation while the main menu is up so neither the
            // player nor the zombies move before "Begin" is pressed.
            Time.timeScale = 0f;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) &&
                (CurrentState == State.Playing || CurrentState == State.Paused))
                TogglePause();
        }

        public void StartGame()
        {
            collectedFragments = 0;
            rememberedMemories.Clear();
            SetState(State.Playing);
            Time.timeScale = 1f;
        }

        public void TogglePause()
        {
            if (CurrentState == State.Playing)
            {
                SetState(State.Paused);
                Time.timeScale = 0f;
            }
            else if (CurrentState == State.Paused)
            {
                SetState(State.Playing);
                Time.timeScale = 1f;
            }
        }

        public void OnFragmentCollected(string memoryText)
        {
            collectedFragments++;
            if (!string.IsNullOrEmpty(memoryText))
                rememberedMemories.Add(memoryText);

            var hud = UI.HUDController.Instance;
            if (hud != null)
            {
                hud.ShowMessage(memoryText, 4f);
                if (AllFragmentsCollected)
                    hud.ShowMessage("All memories recovered. Find the LIGHT.", 4f);
                else
                    hud.RefreshFragmentCount();
            }
        }

        public void WinGame()
        {
            SetState(State.Won);
            Time.timeScale = 0f;
        }

        public void LoseGame()
        {
            if (CurrentState != State.Playing) return;
            SetState(State.Lost);
            Time.timeScale = 0f;
        }

        public void Restart()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        void SetState(State next)
        {
            CurrentState = next;
            OnStateChanged?.Invoke(next);
        }
    }
}
